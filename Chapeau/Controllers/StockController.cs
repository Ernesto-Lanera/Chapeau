using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class StockController : Controller
    {
        private readonly MenuService _menuService;

        public StockController(MenuService menuService)
        {
            _menuService = menuService;
        }

        public IActionResult Index(int? cardId, int? categoryId)
        {
            var menuItems = _menuService.GetMenuItems(cardId, categoryId);
            ViewBag.SelectedCardId = cardId;
            ViewBag.SelectedCategoryId = categoryId;
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