using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chapeau.Services;
using Chapeau.Models;
using System.Text.Json;

namespace Chapeau.Controllers
{
    public class MenuController(MenuService menuService, CategoryService categoryService) : Controller
    {
        private readonly MenuService _menuService = menuService;
        private readonly CategoryService _categoryService = categoryService;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";
        private const string NewMenuItemDraftKey = "NewMenuItemDraft";

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
        public IActionResult QuickCreate(MenuItem item, int? cardId, int? categoryId)
        {
            // Preserve the draft so the inline form can re-render with values.
            TempData[NewMenuItemDraftKey] = JsonSerializer.Serialize(new
            {
                item.Name,
                item.RetailPrice,
                item.Stock,
                item.CategoryID
            });

            // Check for duplicate name (case-insensitive)
            if (_menuService.GetMenuItems(null, null).Any(m =>
                    m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(MenuItem.Name), "Er bestaat al een menu-item met deze naam.");
            }

            if (item.RetailPrice < 0)
            {
                ModelState.AddModelError(nameof(MenuItem.RetailPrice), "Prijs kan niet negatief zijn.");
            }

            if (item.Stock < 0)
            {
                ModelState.AddModelError(nameof(MenuItem.Stock), "Voorraad kan niet negatief zijn.");
            }

            if (item.CategoryID <= 0)
            {
                ModelState.AddModelError(nameof(MenuItem.CategoryID), "Kies een geldige categorie.");
            }

            // Inline create only uses retail price in the UI; keep purchase price safe.
            if (item.PurchasePrice < 0)
            {
                item.PurchasePrice = 0;
            }

            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = string.Join("\n",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }

            try
            {
                _menuService.AddMenuItem(item);
                TempData[FlashSuccessKey] = "Menu-item toegevoegd.";
                TempData.Remove(NewMenuItemDraftKey);
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
            catch (InvalidOperationException ex)
            {
                TempData[FlashErrorKey] = ex.Message;
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is iets misgegaan bij het opslaan. Probeer het opnieuw.";
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var categories = _categoryService.GetCategories();
            ViewBag.Categories = categories;

            // MenuCards (static data)
            var menuCards = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lunch" },
                new SelectListItem { Value = "2", Text = "Diner" },
                new SelectListItem { Value = "3", Text = "Dranken" }
            };
            ViewBag.MenuCards = menuCards;

            return View();
        }

        [HttpPost]
        public IActionResult Create(MenuItem item)
        {
            // Check for duplicate name (case-insensitive)
            if (_menuService.GetMenuItems(null, null).Any(m =>
                    m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "Een menu-item met deze naam bestaat al.");
            }

            // Additional server-side validation
            if (item.PurchasePrice < 0)
            {
                ModelState.AddModelError("PurchasePrice", "Inkoopprijs kan niet negatief zijn.");
            }

            if (item.RetailPrice < 0)
            {
                ModelState.AddModelError("RetailPrice", "Verkoopprijs kan niet negatief zijn.");
            }

            if (item.Stock < 0)
            {
                ModelState.AddModelError("Stock", "Voorraad kan niet negatief zijn.");
            }

            if (item.CategoryID <= 0)
            {
                ModelState.AddModelError("CategoryID", "Selecteer een geldige categorie.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _menuService.AddMenuItem(item);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Er is een onverwachte fout opgetreden bij het opslaan.");
                }
            }

            // Repopulate categories and menu cards for form re-display
            var categoriesForReDisplay = _categoryService.GetCategories();
            ViewBag.Categories = categoriesForReDisplay;

            var menuCardsForReDisplay = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lunch" },
                new SelectListItem { Value = "2", Text = "Diner" },
                new SelectListItem { Value = "3", Text = "Dranken" }
            };
            ViewBag.MenuCards = menuCardsForReDisplay;

            return View(item);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = _menuService.GetMenuItems(null, null)
                .FirstOrDefault(m => m.MenuItemID == id);

            if (item == null)
            {
                return NotFound();
            }

            var categories = _categoryService.GetCategories();
            ViewBag.Categories = categories;

            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(MenuItem item)
        {
            // Check for duplicate name (case-insensitive, excluding current item)
            if (_menuService.GetMenuItems(null, null).Any(m =>
                    m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)
                    && m.MenuItemID != item.MenuItemID))
            {
                ModelState.AddModelError("Name", "Een ander menu-item met deze naam bestaat al.");
            }

            // Additional server-side validation
            if (item.PurchasePrice < 0)
            {
                ModelState.AddModelError("PurchasePrice", "Inkoopprijs kan niet negatief zijn.");
            }

            if (item.RetailPrice < 0)
            {
                ModelState.AddModelError("RetailPrice", "Verkoopprijs kan niet negatief zijn.");
            }

            if (item.Stock < 0)
            {
                ModelState.AddModelError("Stock", "Voorraad kan niet negatief zijn.");
            }

            if (item.CategoryID <= 0)
            {
                ModelState.AddModelError("CategoryID", "Selecteer een geldige categorie.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _menuService.UpdateMenuItem(item);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Er is een onverwachte fout opgetreden bij het opslaan.");
                }
            }

            // Repopulate categories for form re-display
            var categoriesForReDisplay = _categoryService.GetCategories();
            ViewBag.Categories = categoriesForReDisplay;

            return View(item);
        }

        [HttpPost]
        public IActionResult ToggleActive(int id, bool active)
        {
            try
            {
                _menuService.SetMenuItemActive(id, active);
                TempData[FlashSuccessKey] = active ? "Menu-item geactiveerd." : "Menu-item gedeactiveerd.";
            }
            catch
            {
                TempData[FlashErrorKey] = "Kon status niet aanpassen. Probeer opnieuw.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}