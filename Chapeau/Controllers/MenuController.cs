using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanViewMenu")]
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
        public IActionResult Index(int? tableId, int? tableNumber)
        {
            try
            {
                var menuItems = _menuService.GetMenuItems(null, null);
                var allCategories = _categoryService.GetCategories();

                ViewBag.AllCategories = allCategories;
                ViewBag.MenuCards = GetMenuCardSelectList();
                ViewBag.TableId = tableId;
                ViewBag.TableNumber = tableNumber;
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
