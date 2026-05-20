using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Chapeau.Controllers
{
    public class StockController(MenuService menuService, CategoryService categoryService) : Controller
    {
        private readonly MenuService _menuService = menuService;
        private readonly CategoryService _categoryService = categoryService;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";

        // Toont alle menu-items met voorraad overzicht
        public IActionResult Index(int? cardId, int? categoryId)
        {
            var categories = _categoryService.GetCategories();

            // Validate that selected category belongs to selected card
            if (cardId.HasValue && categoryId.HasValue)
            {
                var categoryBelongsToCard = categories.Any(c => 
                    c.CategoryID == categoryId.Value && c.MenuCardID == cardId.Value);

                if (!categoryBelongsToCard)
                    categoryId = null;
            }

            var menuItems = _menuService.GetMenuItems(cardId, categoryId);

            PopulateViewBag(cardId, categoryId, categories);

            return View(menuItems);
        }

        // Werkt voorraad bij voor menu-item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, int newStock, int? cardId, int? categoryId)
        {
            if (!ValidateStockRequest(id, newStock))
                return BadRequest();

            try
            {
                _menuService.ChangeStock(id, newStock);

                if (IsAjaxRequest())
                    return Ok(new { success = true });

                TempData[FlashSuccessKey] = "Voorraad succesvol bijgewerkt.";
            }
            catch (Exception ex)
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, message = ex.Message });

                TempData[FlashErrorKey] = $"Fout bij bijwerken voorraad: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { cardId, categoryId });
        }

        // Valideert voorraad verzoek
        private bool ValidateStockRequest(int id, int stock)
        {
            return id > 0 && stock >= 0;
        }

        // Controleert of verzoek via AJAX komt
        private bool IsAjaxRequest() => 
            Request.Headers["X-Requested-With"] == "fetch";

        // Vult ViewBag met filteropties
        private void PopulateViewBag(int? cardId, int? categoryId, IEnumerable<Category> categories)
        {
            ViewBag.SelectedCardId = cardId;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Categories = categories;
            ViewBag.AllCategories = categories;
            ViewBag.MenuCards = GetMenuCards();
        }

        // Retourneert dropdown list van menukaarten
        private static List<SelectListItem> GetMenuCards()
        {
            return new List<SelectListItem>
            {
                new(MenuCardConstants.LunchCardName, MenuCardConstants.LunchCardId.ToString()),
                new(MenuCardConstants.DinnerCardName, MenuCardConstants.DinnerCardId.ToString()),
                new(MenuCardConstants.DrinksCardName, MenuCardConstants.DrinksCardId.ToString())
            };
        }
    }
}