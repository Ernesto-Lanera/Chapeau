using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chapeau.Services;
using Chapeau.Models;
using System.Globalization;

namespace Chapeau.Controllers
{
    public class ManageMenuController : Controller
    {
        private readonly MenuService _menuService;
        private readonly CategoryService _categoryService;
        private readonly ImageService _imageService;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";

        public ManageMenuController(
            MenuService menuService,
            CategoryService categoryService,
            ImageService imageService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
            _imageService = imageService;
        }

        public IActionResult Index(
            int? cardId,
            int? categoryId,
            int? editId,
            bool showCreate = false,
            string? search = null)
        {
            var allCategories = _categoryService.GetCategories();

            if (cardId.HasValue && categoryId.HasValue)
            {
                var selectedCategory = allCategories
                    .FirstOrDefault(c => c.CategoryID == categoryId.Value);

                if (selectedCategory == null || selectedCategory.MenuCardID != cardId.Value)
                {
                    categoryId = null;
                }
            }

            var menuItems = _menuService.GetMenuItems(cardId, categoryId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchTerm = search.Trim();

                menuItems = menuItems
                    .Where(m => !string.IsNullOrWhiteSpace(m.Name)
                                && m.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var filterCategories = allCategories;

            if (cardId.HasValue)
            {
                filterCategories = allCategories
                    .Where(c => c.MenuCardID == cardId.Value)
                    .ToList();
            }

            ViewBag.SelectedCardId = cardId;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Search = search ?? "";

            ViewBag.AllCategories = allCategories;
            ViewBag.FilterCategories = filterCategories;
            ViewBag.MenuCards = GetMenuCardSelectList();

            ViewBag.IsEdit = false;
            ViewBag.ShowCreate = showCreate;
            ViewBag.SelectedMenuCardId = cardId ?? 0;
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

            return View("~/Views/Menu/Index.cshtml", menuItems);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MenuItem item, IFormFile? imageFile, string? search = null)
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

            if (imageFile != null)
            {
                var (success, path, errorMessage) = await _imageService.UploadImageAsync(imageFile);

                if (!success)
                {
                    ModelState.AddModelError("imageFile", errorMessage ?? "Fout bij upload");
                }
                else if (!string.IsNullOrWhiteSpace(path))
                {
                    item.ImagePath = path;
                }
            }

            ValidateMenuItemForCreate(item, parsedMenuCardId);

            if (ModelState.IsValid)
            {
                try
                {
                    _menuService.AddMenuItem(item);

                    TempData[FlashSuccessKey] = $"Menu-item '{item.Name}' is succesvol toegevoegd!";

                    return RedirectToAction(nameof(Index), new
                    {
                        cardId = parsedMenuCardId,
                        search
                    });
                }
                catch (InvalidOperationException ex)
                {
                    TempData[FlashErrorKey] = ex.Message;

                    return RedirectToAction(nameof(Index), new
                    {
                        cardId = parsedMenuCardId,
                        showCreate = true,
                        search
                    });
                }
                catch
                {
                    TempData[FlashErrorKey] = "Er is een onverwachte fout opgetreden bij het opslaan.";

                    return RedirectToAction(nameof(Index), new
                    {
                        cardId = parsedMenuCardId,
                        showCreate = true,
                        search
                    });
                }
            }

            TempData[FlashErrorKey] = GetModelStateErrors();

            return RedirectToAction(nameof(Index), new
            {
                cardId = parsedMenuCardId,
                showCreate = true,
                search
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MenuItem item, IFormFile? imageFile, string? search = null)
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
                return RedirectToAction(nameof(Index), new { search });
            }

            item.IsActive = existingItem.IsActive;

            if (imageFile != null)
            {
                var (success, path, errorMessage) = await _imageService.UploadImageAsync(imageFile);

                if (!success)
                {
                    ModelState.AddModelError("imageFile", errorMessage ?? "Fout bij upload");
                }
                else if (!string.IsNullOrWhiteSpace(path))
                {
                    item.ImagePath = path;
                }
            }
            else
            {
                item.ImagePath = existingItem.ImagePath;
            }

            ValidateMenuItemForEdit(item);

            if (ModelState.IsValid)
            {
                try
                {
                    _menuService.UpdateMenuItem(item);

                    TempData[FlashSuccessKey] = $"Menu-item '{item.Name}' is succesvol bijgewerkt!";

                    return RedirectToAction(nameof(Index), new { search });
                }
                catch (InvalidOperationException ex)
                {
                    TempData[FlashErrorKey] = ex.Message;

                    return RedirectToAction(nameof(Index), new
                    {
                        editId = item.MenuItemID,
                        search
                    });
                }
                catch
                {
                    TempData[FlashErrorKey] = "Er is een onverwachte fout opgetreden bij het opslaan.";

                    return RedirectToAction(nameof(Index), new
                    {
                        editId = item.MenuItemID,
                        search
                    });
                }
            }

            TempData[FlashErrorKey] = GetModelStateErrors();

            return RedirectToAction(nameof(Index), new
            {
                editId = item.MenuItemID,
                search
            });
        }

        [HttpPost]
        public IActionResult ToggleActive(int id, bool active, int? cardId, int? categoryId, string? search = null)
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
                categoryId,
                search
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
                ModelState.AddModelError(nameof(MenuItem.RetailPrice), "Verkoopsprijs kan niet negatief zijn.");
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

            bool hasDot = trimmed.Contains('.');
            bool hasComma = trimmed.Contains(',');

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

            string normalized = trimmed.Replace(',', '.');

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