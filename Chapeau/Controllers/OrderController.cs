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



        public IActionResult Index(int? tableId, int? tableNumber, string? guestNames = null)
        {
            try
            {
                ViewBag.TableNumber = tableNumber;
                ViewBag.AllCategories = _categoryService.GetCategories();
                ViewBag.MenuCards = GetMenuCardSelectList();
                var menuItems = _menuService.GetMenuItems(null, null) ?? new List<MenuItem>();
                ViewBag.StockLevelsJson = JsonSerializer.Serialize(menuItems.ToDictionary(m => m.MenuItemID, m => m.Stock));

                return View(menuItems);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Could not load the menu. Please try again.";
                return View("Error");
            }
        }

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

                Order order = new Order
                {
                    TableId = payload.TableId,
                    OrderDate = DateTime.Now,
                    Items = new List<OrderItem>()
                };

                foreach (var jsItem in payload.Items)
                {
                    if (jsItem.Amount <= 0) continue;

                    var newOrderItem = new OrderItem
                    {
                        MenuItemId = jsItem.MenuItemId,
                        Amount = jsItem.Amount,
                        Comment = jsItem.Comment,
                        OrderItemStatus = 0
                    };
                    order.Items.Add(newOrderItem);
                }

                _orderService.SaveOrderToDb(order);

                this.ShowNotification("Your Order was saved successfully!", "success");

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Table") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "A database error occurred while saving. Please try submitting again., Db Erros :" + ex.Message });
            }
        }

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