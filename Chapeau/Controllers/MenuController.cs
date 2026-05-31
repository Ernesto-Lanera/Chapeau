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

        private Order GetOrder()
        {
            var sessionData = HttpContext.Session.GetString("ActiveOrder");
            if (string.IsNullOrEmpty(sessionData))
            {
                return _orderService.MakeNewOrder(1);
            }
            return JsonSerializer.Deserialize<Order>(sessionData);
        }

        private void SaveOrder(Order order)
        {
            HttpContext.Session.SetString("ActiveOrder", JsonSerializer.Serialize(order));
        }

        public IActionResult Index()
        {
            try
            {
                ViewBag.Order = GetOrder();
                ViewBag.AllCategories = _categoryService.GetCategories();
                ViewBag.MenuCards = GetMenuCardSelectList();
                var menuItems = _menuService.GetMenuItems(null, null) ?? new List<MenuItem>(); ;

                return View(menuItems);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult AddMenuItemToOrder(int MenuCardId, string MenuItemName)
        {
            Order order = GetOrder();
            order = _orderService.AddOrderItemToOrder(MenuCardId, order, MenuItemName);
            SaveOrder(order);
            return Json(new { success = true, items = order.OrderItems.Values });
        }

        [HttpPost]
        public IActionResult RemoveMenuItemFromOrder(int MenuCardId)
        {
            Order order = GetOrder();
            order = _orderService.RemoveItemFormOrder(MenuCardId, order);
            SaveOrder(order);
            return Json(new { success = true, items = order.OrderItems.Values });
        }

        [HttpPost]
        public IActionResult UpdateMenuItemQuantity(int MenuCardId, int NewQuantity)
        {
            Order order = GetOrder();
            order = _orderService.UpdateItemFormOrder(MenuCardId, order, NewQuantity);
            SaveOrder(order);
            return Json(new { success = true, items = order.OrderItems.Values });
        }

        [HttpPost]
        public IActionResult AddCommentToItem(int MenuCardId, string Comment)
        {
            Order order = GetOrder();
            order = _orderService.AddCommentoItem(MenuCardId, order, Comment);
            SaveOrder(order);
            return Json(new { success = true, items = order.OrderItems.Values });
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