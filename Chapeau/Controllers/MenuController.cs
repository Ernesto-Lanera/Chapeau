using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Menu controller for displaying the restaurant menu to customers.
    /// </summary>
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly ICategoryService _categoryService;

        public MenuController(
            IMenuService menuService,
            ICategoryService categoryService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
        }

        /// <summary>
        /// Display the full menu with all items, filterable by category.
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                var menuItems = _menuService.GetMenuItems(null, null);
                var allCategories = _categoryService.GetCategories();

                ViewBag.AllCategories = allCategories;
                ViewBag.MenuCards = GetMenuCardSelectList();
                return View(menuItems);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
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
