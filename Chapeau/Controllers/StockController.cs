using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class StockController(MenuService menuService, CategoryService categoryService) : Controller
    {
        private readonly MenuService _menuService = menuService;
        private readonly CategoryService _categoryService = categoryService;

        public IActionResult Index(int? cardId, int? categoryId)
        {
            var menuItems = _menuService.GetMenuItems(cardId, categoryId);
            var categories = _categoryService.GetCategories();

            ViewBag.SelectedCardId = cardId;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Categories = categories;

            return View(menuItems);
        }

        [HttpPost]
        public IActionResult Update(int id, int newStock)
        {
            _menuService.ChangeStock(id, newStock);
            return RedirectToAction(nameof(Index));
        }
    }
}