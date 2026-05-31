using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Chapeau.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;
        private readonly CategoryService _categoryService;
        private Order order => MakeNewOrder();



        public MenuController(
           MenuService menuService,
           CategoryService categoryService,
            OrderService orderService
           )
        {
            _menuService = menuService;
            _orderService = orderService;
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            var menuItems = _menuService.GetMenuItems(null, null);
            var allCategories = _categoryService.GetCategories();

           

            ViewBag.Order = order;
            ViewBag.AllCategories = allCategories;
            ViewBag.MenuCards = GetMenuCardSelectList();
            return View(menuItems);
        }

        [HttpPost]
        public IActionResult AddMenuItemToOrder()
        {
            return View(order);
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

        private Order MakeNewOrder()
        {
            int TableId = 1; // temp value until i get send table id form table page
            var order = _orderService.MakeNewOrder(TableId);

            return order;
        }
    }

}
