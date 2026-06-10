using Chapeau.Constants;
using Chapeau.Extensions;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Chapeau.Controllers
{
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
                    HttpContext.Session.Remove("ActiveOrder");
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

        private void SaveOrdertoJson(Order order)
        {
            HttpContext.Session.SetString("ActiveOrder", JsonSerializer.Serialize(order));
        }

        public IActionResult Index(int? tableId, int? tableNumber, string? guestNames = null)
        {
            try
            {
                var order = GetOrder(tableId, tableNumber, guestNames);
                SaveOrdertoJson(order);
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
            SaveOrdertoJson(order);
            return Json(new { success = true, items = order.OrderItems });
        }

        [HttpPost]
        public IActionResult RemoveMenuItemFromOrder(int MenuItemId)
        {
            Order order = GetOrder();
            order = _orderService.RemoveItemFromOrder(MenuItemId, order);
            SaveOrdertoJson(order);
            return Json(new { success = true, items = order.OrderItems});
        }

        [HttpPost]
        public IActionResult UpdateMenuItemQuantity(int MenuItemId, int NewQuantity)
        {
            Order order = GetOrder();
            order = _orderService.UpdateItemFromOrder(MenuItemId, order, NewQuantity);
            SaveOrdertoJson(order);
            return Json(new { success = true, items = order.OrderItems });
        }

        [HttpPost]
        public IActionResult UpdateItemComment(int MenuItemId, string Comment)
        {
            Order order = GetOrder();
            order = _orderService.UpdateItemComment(MenuItemId, order, Comment);
            SaveOrdertoJson(order);
            return Json(new { success = true, items = order.OrderItems });
        }

        [HttpPost]
        public IActionResult SaveOrderToDb()
        {
            Order order = GetOrder();
            _orderService.SaveOrderToDb(order);
             this.ShowNotification("Your Order was saved successfully!", "success");
            return RedirectToAction("Index", "Table");
           
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            return RedirectToAction("Index", "Table");
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