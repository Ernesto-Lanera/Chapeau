using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class StockController(MenuService menuService, CategoryService categoryService) : Controller
    {
        private readonly MenuService _menuService = menuService;
        private readonly CategoryService _categoryService = categoryService;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";

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
        public IActionResult Update(int id, int newStock, int? cardId, int? categoryId)
        {
            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(Request.Headers["X-Requested-With"], "fetch", StringComparison.OrdinalIgnoreCase)
                         || Request.Headers.Accept.Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase));

            try
            {
                _menuService.ChangeStock(id, newStock);
                if (isAjax)
                    return Ok(new { id, newStock });

                TempData[FlashSuccessKey] = "Voorraad bijgewerkt.";
            }
            catch (ArgumentException ex)
            {
                if (isAjax)
                    return BadRequest(new { error = ex.Message });

                TempData[FlashErrorKey] = ex.Message;
            }
            catch
            {
                if (isAjax)
                    return StatusCode(500, new { error = "Kon voorraad niet bijwerken. Probeer opnieuw." });

                TempData[FlashErrorKey] = "Kon voorraad niet bijwerken. Probeer opnieuw.";
            }

            return RedirectToAction(nameof(Index), new { cardId, categoryId });
        }
    }
}