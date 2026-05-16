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

        public IActionResult Index(int? cardId, int? categoryId)
        {
            try
            {
                var categories = _categoryService.GetCategories();

                if (cardId.HasValue && categoryId.HasValue)
                {
                    bool categoryBelongsToCard = categories.Any(category =>
                        category.CategoryID == categoryId.Value &&
                        category.MenuCardID == cardId.Value);

                    if (!categoryBelongsToCard)
                    {
                        categoryId = null;
                    }
                }

                var menuItems = _menuService.GetMenuItems(cardId, categoryId);

                ViewBag.SelectedCardId = cardId;
                ViewBag.SelectedCategoryId = categoryId;

                ViewBag.Categories = categories;
                ViewBag.AllCategories = categories;
                ViewBag.FilterCategories = categories;
                ViewBag.MenuCards = GetMenuCards();

                return View(menuItems);
            }
            catch (Exception ex)
            {
                TempData[FlashErrorKey] = $"Fout bij laden voorraad: {ex.Message}";

                ViewBag.SelectedCardId = cardId;
                ViewBag.SelectedCategoryId = categoryId;

                ViewBag.Categories = new List<Category>();
                ViewBag.AllCategories = new List<Category>();
                ViewBag.FilterCategories = new List<Category>();
                ViewBag.MenuCards = GetMenuCards();

                return View(new List<MenuItem>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, int newStock, int? cardId, int? categoryId)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest();
                }

                if (newStock < 0)
                {
                    return BadRequest();
                }

                _menuService.ChangeStock(id, newStock);

                if (Request.Headers["X-Requested-With"] == "fetch")
                {
                    return Ok(new { success = true });
                }

                TempData[FlashSuccessKey] = "Voorraad succesvol bijgewerkt.";
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "fetch")
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }

                TempData[FlashErrorKey] = $"Fout bij bijwerken voorraad: {ex.Message}";
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStock(int id, int stock, int? cardId, int? categoryId)
        {
            try
            {
                if (id <= 0)
                {
                    TempData[FlashErrorKey] = "Ongeldig menu item.";
                    return RedirectToAction(nameof(Index), new { cardId, categoryId });
                }

                if (stock < 0)
                {
                    TempData[FlashErrorKey] = "Voorraad mag niet negatief zijn.";
                    return RedirectToAction(nameof(Index), new { cardId, categoryId });
                }

                _menuService.ChangeStock(id, stock);

                TempData[FlashSuccessKey] = "Voorraad succesvol bijgewerkt.";
            }
            catch (Exception ex)
            {
                TempData[FlashErrorKey] = $"Fout bij bijwerken voorraad: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { cardId, categoryId });
        }

        private List<SelectListItem> GetMenuCards()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "1",
                    Text = "Lunch"
                },
                new SelectListItem
                {
                    Value = "2",
                    Text = "Diner"
                },
                new SelectListItem
                {
                    Value = "3",
                    Text = "Dranken"
                }
            };
        }
    }
}