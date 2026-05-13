using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chapeau.Services;
using Chapeau.Models;
using System.Text.Json;
using System.Globalization;

namespace Chapeau.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;
        private readonly CategoryService _categoryService;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";
        private const string NewMenuItemDraftKey = "NewMenuItemDraft";

        public MenuController(MenuService menuService, CategoryService categoryService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
        }

        public IActionResult Index(int? cardId, int? categoryId, int? editId, bool showCreate = false)
        {
            var menuItems = _menuService.GetMenuItems(cardId, categoryId);
            var allCategories = _categoryService.GetCategories();

            ViewBag.SelectedCardId = cardId;
            ViewBag.SelectedCategoryId = categoryId;

            ViewBag.AllCategories = allCategories;
            ViewBag.FilterCategories = allCategories;
            ViewBag.MenuCards = GetMenuCardSelectList();

            ViewBag.IsEdit = false;
            ViewBag.ShowCreate = showCreate;
            ViewBag.SelectedMenuCardId = 0;
            ViewBag.EditItem = null;
            ViewBag.Categories = allCategories;

            if (editId.HasValue)
            {
                var editItem = _menuService.GetMenuItems(null, null)
                    .FirstOrDefault(m => m.MenuItemID == editId.Value);

                if (editItem != null)
                {
                    var currentCategory = allCategories
                        .FirstOrDefault(c => c.CategoryID == editItem.CategoryID);

                    int selectedMenuCardId = currentCategory?.MenuCardID ?? 0;

                    var filteredCategories = allCategories
                        .Where(c => c.MenuCardID == selectedMenuCardId)
                        .ToList();

                    ViewBag.EditItem = editItem;
                    ViewBag.IsEdit = true;
                    ViewBag.ShowCreate = false;
                    ViewBag.SelectedMenuCardId = selectedMenuCardId;
                    ViewBag.Categories = filteredCategories;
                }
            }

            return View(menuItems);
        }

        [HttpPost]
        public IActionResult QuickCreate(MenuItem item, int? cardId, int? categoryId)
        {
            int parsedCategoryId = ParseCategoryIdFromForm();
            int parsedMenuCardId = ParseMenuCardIdFromForm();

            item.CategoryID = parsedCategoryId;
            item.IsActive = true;

            ModelState.Remove(nameof(MenuItem.CategoryID));
            ModelState.Remove("CategoryID");
            ModelState.Remove("CategoryIDSelect");

            ModelState.Remove(nameof(MenuItem.RetailPrice));
            ModelState.Remove("RetailPrice");

            ModelState.Remove(nameof(MenuItem.Stock));
            ModelState.Remove("Stock");

            var rawRetailPrice = Request.Form["RetailPrice"].FirstOrDefault();

            if (!TryParseDecimalFlexible(rawRetailPrice, out var parsedRetailPrice))
            {
                parsedRetailPrice = 0m;
                ModelState.AddModelError(nameof(MenuItem.RetailPrice), "Vul een geldige prijs in.");
            }

            item.RetailPrice = parsedRetailPrice;

            var rawStock = Request.Form["Stock"].FirstOrDefault();

            if (!int.TryParse(rawStock, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedStock))
            {
                parsedStock = 0;
                ModelState.AddModelError(nameof(MenuItem.Stock), "Vul een geldige voorraad in.");
            }

            item.Stock = parsedStock;

            TempData[NewMenuItemDraftKey] = JsonSerializer.Serialize(new
            {
                item.Name,
                item.RetailPrice,
                item.Stock,
                item.CategoryID,
                MenuCardID = parsedMenuCardId
            });

            ValidateMenuItemForCreate(item, parsedMenuCardId);

            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = GetModelStateErrors();

                return RedirectToAction(nameof(Index), new
                {
                    cardId,
                    categoryId,
                    showCreate = true
                });
            }

            try
            {
                _menuService.AddMenuItem(item);

                TempData[FlashSuccessKey] = "Menu-item toegevoegd.";
                TempData.Remove(NewMenuItemDraftKey);

                // Succes: form sluiten
                return RedirectToAction(nameof(Index), new
                {
                    cardId = parsedMenuCardId
                });
            }
            catch (InvalidOperationException ex)
            {
                TempData[FlashErrorKey] = ex.Message;

                // Fout: form open houden
                return RedirectToAction(nameof(Index), new
                {
                    cardId,
                    categoryId,
                    showCreate = true
                });
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is iets misgegaan bij het opslaan. Probeer het opnieuw.";

                // Fout: form open houden
                return RedirectToAction(nameof(Index), new
                {
                    cardId,
                    categoryId,
                    showCreate = true
                });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return RedirectToAction(nameof(Index), new
            {
                showCreate = true
            });
        }

        [HttpPost]
        public IActionResult Create(MenuItem item)
        {
            ModelState.Remove(nameof(MenuItem.RetailPrice));
            ModelState.Remove("RetailPrice");

            var rawRetailPrice = Request.Form["RetailPrice"].FirstOrDefault();

            if (!TryParseDecimalFlexible(rawRetailPrice, out var parsedRetailPrice))
            {
                parsedRetailPrice = 0m;
                ModelState.AddModelError(nameof(MenuItem.RetailPrice), "Vul een geldige prijs in.");
            }

            item.RetailPrice = parsedRetailPrice;
            item.IsActive = true;

            int parsedMenuCardId = ParseMenuCardIdFromForm();

            ValidateMenuItemForCreate(item, parsedMenuCardId);

            if (ModelState.IsValid)
            {
                try
                {
                    _menuService.AddMenuItem(item);

                    TempData[FlashSuccessKey] = $"Menu-item '{item.Name}' is succesvol toegevoegd!";

                    // Succes: form sluiten
                    return RedirectToAction(nameof(Index), new
                    {
                        cardId = parsedMenuCardId
                    });
                }
                catch (InvalidOperationException ex)
                {
                    TempData[FlashErrorKey] = ex.Message;

                    // Fout: form open houden
                    return RedirectToAction(nameof(Index), new
                    {
                        cardId = parsedMenuCardId,
                        showCreate = true
                    });
                }
                catch
                {
                    TempData[FlashErrorKey] = "Er is een onverwachte fout opgetreden bij het opslaan.";

                    // Fout: form open houden
                    return RedirectToAction(nameof(Index), new
                    {
                        cardId = parsedMenuCardId,
                        showCreate = true
                    });
                }
            }

            TempData[FlashErrorKey] = GetModelStateErrors();

            // Validatiefout: form open houden
            return RedirectToAction(nameof(Index), new
            {
                cardId = parsedMenuCardId,
                showCreate = true
            });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return RedirectToAction(nameof(Index), new
            {
                editId = id
            });
        }

        [HttpPost]
        public IActionResult Edit(MenuItem item)
        {
            ModelState.Remove(nameof(MenuItem.RetailPrice));
            ModelState.Remove("RetailPrice");

            var rawRetailPrice = Request.Form["RetailPrice"].FirstOrDefault();

            if (!TryParseDecimalFlexible(rawRetailPrice, out var parsedRetailPrice))
            {
                parsedRetailPrice = 0m;
                ModelState.AddModelError(nameof(MenuItem.RetailPrice), "Vul een geldige prijs in.");
            }

            item.RetailPrice = parsedRetailPrice;

            var existingItem = _menuService.GetMenuItems(null, null)
                .FirstOrDefault(m => m.MenuItemID == item.MenuItemID);

            if (existingItem == null)
            {
                TempData[FlashErrorKey] = "Menu-item bestaat niet meer.";
                return RedirectToAction(nameof(Index));
            }

            // Status behouden. Actief/inactief gaat via ToggleActive.
            item.IsActive = existingItem.IsActive;

            ValidateMenuItemForEdit(item);

            if (ModelState.IsValid)
            {
                try
                {
                    _menuService.UpdateMenuItem(item);

                    TempData[FlashSuccessKey] = $"Menu-item '{item.Name}' is succesvol bijgewerkt!";

                    // Succes: form sluiten, dus GEEN editId meegeven
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    TempData[FlashErrorKey] = ex.Message;

                    // Fout: edit-form open houden
                    return RedirectToAction(nameof(Index), new
                    {
                        editId = item.MenuItemID
                    });
                }
                catch
                {
                    TempData[FlashErrorKey] = "Er is een onverwachte fout opgetreden bij het opslaan.";

                    // Fout: edit-form open houden
                    return RedirectToAction(nameof(Index), new
                    {
                        editId = item.MenuItemID
                    });
                }
            }

            TempData[FlashErrorKey] = GetModelStateErrors();

            // Validatiefout: edit-form open houden
            return RedirectToAction(nameof(Index), new
            {
                editId = item.MenuItemID
            });
        }

        [HttpPost]
        public IActionResult ToggleActive(int id, bool active, int? cardId, int? categoryId)
        {
            try
            {
                _menuService.SetMenuItemActive(id, active);

                TempData[FlashSuccessKey] = active
                    ? "Menu-item geactiveerd."
                    : "Menu-item gedeactiveerd.";
            }
            catch
            {
                TempData[FlashErrorKey] = "Kon status niet aanpassen. Probeer opnieuw.";
            }

            return RedirectToAction(nameof(Index), new
            {
                cardId,
                categoryId
            });
        }

        private void ValidateMenuItemForCreate(MenuItem item, int menuCardId)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                ModelState.AddModelError(nameof(MenuItem.Name), "Naam is verplicht.");
            }

            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                bool duplicateExists = _menuService.GetMenuItems(null, null)
                    .Any(m => m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

                if (duplicateExists)
                {
                    ModelState.AddModelError(nameof(MenuItem.Name), "Er bestaat al een menu-item met deze naam.");
                }
            }

            ValidateBasicMenuItemFields(item);
            ValidateCategoryForCreate(item.CategoryID, menuCardId);
        }

        private void ValidateMenuItemForEdit(MenuItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                ModelState.AddModelError(nameof(MenuItem.Name), "Naam is verplicht.");
            }

            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                bool duplicateExists = _menuService.GetMenuItems(null, null)
                    .Any(m =>
                        m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)
                        && m.MenuItemID != item.MenuItemID
                    );

                if (duplicateExists)
                {
                    ModelState.AddModelError(nameof(MenuItem.Name), "Een ander menu-item met deze naam bestaat al.");
                }
            }

            ValidateBasicMenuItemFields(item);
            ValidateCategoryForEdit(item);
        }

        private void ValidateBasicMenuItemFields(MenuItem item)
        {
            if (item.RetailPrice < 0)
            {
                ModelState.AddModelError(nameof(MenuItem.RetailPrice), "Verkoopprijs kan niet negatief zijn.");
            }

            if (item.Stock < 0)
            {
                ModelState.AddModelError(nameof(MenuItem.Stock), "Voorraad kan niet negatief zijn.");
            }

            if (item.CategoryID <= 0)
            {
                ModelState.AddModelError(nameof(MenuItem.CategoryID), "Selecteer een geldige categorie.");
            }
        }

        private void ValidateCategoryForCreate(int categoryId, int menuCardId)
        {
            if (menuCardId is not (1 or 2 or 3))
            {
                ModelState.AddModelError("MenuCardID", "Kies een geldige kaart.");
                return;
            }

            if (categoryId <= 0)
            {
                return;
            }

            var category = _categoryService.GetCategories()
                .FirstOrDefault(c => c.CategoryID == categoryId);

            if (category == null)
            {
                ModelState.AddModelError(nameof(MenuItem.CategoryID), "Kies een geldige categorie.");
                return;
            }

            if (category.MenuCardID != menuCardId)
            {
                ModelState.AddModelError(nameof(MenuItem.CategoryID), "Deze categorie hoort niet bij de gekozen kaart.");
            }
        }

        private void ValidateCategoryForEdit(MenuItem item)
        {
            if (item.CategoryID <= 0)
            {
                return;
            }

            var categories = _categoryService.GetCategories();

            var newCategory = categories.FirstOrDefault(c => c.CategoryID == item.CategoryID);

            if (newCategory == null)
            {
                ModelState.AddModelError(nameof(MenuItem.CategoryID), "Kies een geldige categorie.");
                return;
            }

            var existingItem = _menuService.GetMenuItems(null, null)
                .FirstOrDefault(m => m.MenuItemID == item.MenuItemID);

            if (existingItem == null)
            {
                ModelState.AddModelError("", "Menu-item bestaat niet meer.");
                return;
            }

            var oldCategory = categories.FirstOrDefault(c => c.CategoryID == existingItem.CategoryID);

            if (oldCategory == null)
            {
                ModelState.AddModelError("", "Huidige categorie van dit menu-item bestaat niet meer.");
                return;
            }

            if (newCategory.MenuCardID != oldCategory.MenuCardID)
            {
                ModelState.AddModelError(nameof(MenuItem.CategoryID), "Je mag alleen een categorie kiezen binnen dezelfde kaart.");
            }
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

        private int ParseCategoryIdFromForm()
        {
            var rawCategoryId = Request.Form["CategoryID"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(rawCategoryId))
            {
                rawCategoryId = Request.Form["CategoryIDSelect"].FirstOrDefault();
            }

            if (int.TryParse(rawCategoryId, out var parsedCategoryId))
            {
                return parsedCategoryId;
            }

            return 0;
        }

        private int ParseMenuCardIdFromForm()
        {
            var rawMenuCardId = Request.Form["MenuCardID"].FirstOrDefault();

            if (int.TryParse(rawMenuCardId, out var parsedMenuCardId))
            {
                return parsedMenuCardId;
            }

            return 0;
        }

        private static bool TryParseDecimalFlexible(string? input, out decimal value)
        {
            value = 0m;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var trimmed = input.Trim();
            var hasDot = trimmed.Contains('.');
            var hasComma = trimmed.Contains(',');

            if (hasDot && !hasComma)
            {
                return decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            }

            if (hasComma && !hasDot)
            {
                return decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.GetCultureInfo("nl-NL"), out value);
            }

            if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.GetCultureInfo("nl-NL"), out value))
            {
                return true;
            }

            if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.GetCultureInfo("en-US"), out value))
            {
                return true;
            }

            var normalized = trimmed.Replace(',', '.');

            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private string GetModelStateErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .Where(msg => !msg.Contains("The value '' is invalid.", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();

            return string.Join("\n", errors);
        }
    }
}