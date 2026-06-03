using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanViewMenu")]
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;
        private readonly CategoryService _categoryService;

        public MenuController(MenuService menuService, CategoryService categoryService, OrderService orderService)
        {
            _menuService = menuService;
            _orderService = orderService;
            _categoryService = categoryService;
        }

        private Order GetOrder(int? tableId = null, int? tableNumber = null, string? guestNames = null)
        {
            var sessionData = HttpContext.Session.GetString("ActiveOrder");
            if (!string.IsNullOrEmpty(sessionData))
            {
                var existingOrder = JsonSerializer.Deserialize<Order>(sessionData)!;
                if (tableId.HasValue && existingOrder.TableId != tableId.Value)
                {
                    sessionData = null;
                }
            }

            if (string.IsNullOrEmpty(sessionData))
            {
                var order = _orderService.MakeNewOrder(tableId ?? 1);
                order.TableNumber = tableNumber ?? 0;
                order.GuestName = guestNames;
                return order;
            }

            return JsonSerializer.Deserialize<Order>(sessionData)!;
        }

        private void SaveOrder(Order order)
        {
            HttpContext.Session.SetString("ActiveOrder", JsonSerializer.Serialize(order));
        }

        public IActionResult Index(int? tableId, int? tableNumber, string? guestNames = null)
        {
            try
            {
                var order = GetOrder(tableId, tableNumber, guestNames);
                ViewBag.Order = order;
                ViewBag.TableNumber = tableNumber ?? order.TableNumber;
                ViewBag.AllCategories = _categoryService.GetCategories();
                ViewBag.MenuCards = GetMenuCardSelectList();
                var menuItems = _menuService.GetMenuItems(null, null) ?? new List<MenuItem>();

                return View(menuItems);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult GetActiveOrderItems()
        {
            Order order = GetOrder();
            return Json(new { success = true, items = order.OrderItems});
        }

        [HttpPost]
        public IActionResult AddMenuItemToOrder(int MenuItemId, string MenuItemName)
        {
            Order order = GetOrder();
            order = _orderService.AddOrderItemToOrder(MenuItemId, order, MenuItemName);
            SaveOrder(order);
            return Json(new { success = true, items = order.OrderItems });
        }

        [HttpPost]
        public IActionResult RemoveMenuItemFromOrder(int MenuItemId)
        {
            Order order = GetOrder();
            order = _orderService.RemoveItemFormOrder(MenuItemId, order);
            SaveOrder(order);
            return Json(new { success = true, items = order.OrderItems});
        }

        [HttpPost]
        public IActionResult UpdateMenuItemQuantity(int MenuItemId, int NewQuantity)
        {
            Order order = GetOrder();
            order = _orderService.UpdateItemFormOrder(MenuItemId, order, NewQuantity);
            SaveOrder(order);
            return Json(new { success = true, items = order.OrderItems });
        }

        [HttpPost]
        public IActionResult AddCommentToItem(int MenuItemId, string Comment)
        {
            Order order = GetOrder();
            order = _orderService.AddCommentoItem(MenuItemId, order, Comment);
            SaveOrder(order);
            return Json(new { success = true, items = order.OrderItems });
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            try
            {
                Order order = GetOrder();
                if (order.OrderItems == null || !order.OrderItems.Any())
                    return Json(new { success = false, message = "Geen items in de bestelling." });

                _orderService.SaveOrderToDb(order);
                HttpContext.Session.Remove("ActiveOrder");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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