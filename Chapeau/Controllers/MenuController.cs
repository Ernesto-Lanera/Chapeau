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

        private Order GetOrder()
        {
            var sessionData = HttpContext.Session.GetString("ActiveOrder");
            if (string.IsNullOrEmpty(sessionData))
            {
                return _orderService.MakeNewOrder(1);
            }


            return JsonSerializer.Deserialize<Order>(sessionData)!;
        }

        private void SaveOrdertoJson(Order order)
        {
            HttpContext.Session.SetString("ActiveOrder", JsonSerializer.Serialize(order));
        }

        public IActionResult Index()
        {
            try
            {
                HttpContext.Session.SetString("ActiveOrder", "");
                ViewBag.Order = GetOrder();
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
            order = _orderService.ChangeCommentinItem(MenuItemId, order, Comment);
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