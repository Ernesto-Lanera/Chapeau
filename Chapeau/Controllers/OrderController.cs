using Chapeau.Constants;
using Chapeau.Emums;
using Chapeau.Extensions;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Handles menu browsing, item selection, and order creation/editing for waitstaff.
    /// </summary>
    [Authorize(Policy = "CanTakeOrders")]
    public class OrderController : Controller
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;
        private readonly CategoryService _categoryService;

        public OrderController(MenuService menuService, CategoryService categoryService, OrderService orderService)
        {
            _menuService = menuService;
            _orderService = orderService;
            _categoryService = categoryService;
        }

        /// <summary>
        /// Displays the menu page for a given table. If an orderId is provided, pre-loads existing order items for editing.
        /// </summary>
        public IActionResult Index(int? tableId, int? tableNumber, int? orderId, string? guestNames = null)
        {
            try
            {
                ViewBag.TableNumber = tableNumber;
                ViewBag.OrderId = orderId;
                ViewBag.AllCategories = _categoryService.GetCategories();
                ViewBag.MenuCards = GetMenuCardSelectList();
                var menuItems = _menuService.GetMenuItems(null, null) ?? new List<MenuItem>();

                string? existingCartJson = null;
                if (orderId.HasValue)
                {
                    var existingOrder = _orderService.GetActiveOrderWithItemsByTableId(tableId ?? 0);
                    if (existingOrder != null && existingOrder.Items.Count > 0)
                    {
                        var cartItems = existingOrder.Items.Select(i => new
                        {
                            menuItemId = i.MenuItemId,
                            menuItemName = i.MenuItemName,
                            amountOrdered = i.AmountOrdered,
                            comment = i.Comment ?? ""
                        }).ToList();
                        existingCartJson = JsonSerializer.Serialize(cartItems);
                    }
                }
                ViewBag.ExistingCartJson = existingCartJson;

                return View(menuItems);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Could not load the menu. Please try again.";
                return View("Error");
            }
        }

        /// <summary>
        /// Saves a new order or updates an existing order based on whether OrderId is set in the payload.
        /// </summary>
        [HttpPost]
        public IActionResult SaveOrderToDb([FromBody] FinalOrderPayload payload)
        {
            try
            {
                if (payload == null || payload.Items == null || !payload.Items.Any())
                {
                    return Json(new { success = false, message = "Cannot submit an empty order." });
                }


                if (payload.TableId <= 0)
                {
                    return Json(new { success = false, message = "Invalid Table selected. Please go back to the table overview and try again." });
                }

                var newItems = new List<OrderItem>();
                foreach (var jsItem in payload.Items)
                {
                    if (jsItem.Amount <= 0) continue;

                    newItems.Add(new OrderItem
                    {
                        MenuItemId = jsItem.MenuItemId,
                        Amount = jsItem.Amount,
                        Comment = jsItem.Comment,
                        OrderItemStatus = 0
                    });
                }

                if (payload.OrderId.HasValue)
                {
                    var existingOrder = _orderService.GetActiveOrderWithItemsByTableId(payload.TableId);
                    if (existingOrder == null)
                    {
                        return Json(new { success = false, message = "The order you were editing no longer exists." });
                    }

                    var oldItems = existingOrder.Items.ToList();
                    _orderService.UpdateExistingOrder(payload.OrderId.Value, newItems, oldItems);
                    this.ShowNotification("Your Order was updated successfully!", "success");
                }
                else
                {
                    Order order = new Order
                    {
                        TableId = payload.TableId,
                        OrderDate = DateTime.Now,
                        Items = newItems
                    };

                    _orderService.SaveOrderToDb(order);
                    this.ShowNotification("Your Order was saved successfully!", "success");
                }

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Table") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "A database error occurred while saving. Please try submitting again., Db Erros :" + ex.Message  });
            }
        }

        /// <summary>Builds the select list of available menu cards for the filter bar.</summary>
        private static List<SelectListItem> GetMenuCardSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = MenuCardConstants.LunchCardId.ToString(), Text = MenuCardConstants.LunchCardName },
                new SelectListItem { Value = MenuCardConstants.DinnerCardId.ToString(), Text = MenuCardConstants.DinnerCardName },
                new SelectListItem { Value = MenuCardConstants.DrinksCardId.ToString(), Text = MenuCardConstants.DrinksCardName }
            };
        }
    }
}