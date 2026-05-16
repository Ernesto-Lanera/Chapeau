using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Chapeau.Controllers
{
    public class MenuController : Controller
    {

        private readonly MenuService _menuService;
        private readonly CategoryService _categoryService;
        private readonly ImageService _imageService;


        public MenuController(
           MenuService menuService,
           CategoryService categoryService,
           ImageService imageService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
            _imageService = imageService;
        }

        public IActionResult Index()
        {
            List<MenuItem> menuItems = _menuService.GetMenuItems(null , null);

            var allCategories = _categoryService.GetCategories();

            ViewBag.AllCategories = allCategories;
            ViewBag.MenuCards = GetMenuCardSelectList();
            return View(menuItems);
        }

        private static List<SelectListItem> GetMenuCardSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lunch" },
                new SelectListItem { Value = "2", Text = "Diner" },
                new SelectListItem { Value = "3", Text = "Dranken" }
            };
        }
    }
}
