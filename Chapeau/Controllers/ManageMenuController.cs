using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace Chapeau.Controllers
{
    public class ManageMenuController : Controller
    {
        private readonly MenuService _menuService;
        private readonly CategoryService _categoryService;
        private readonly ImageService _imageService;

        public ManageMenuController(
            MenuService menuService,
            CategoryService categoryService,
            ImageService imageService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
            _imageService = imageService;
        }

        // Toont alle menu-items met filter opties
        public IActionResult Index(int? cardId, int? categoryId, int? editId, bool showCreate = false)
        {
            var allCategories = _categoryService.GetCategories();
            (cardId, categoryId) = ValidateSelectedCategory(allCategories, cardId, categoryId);

            var menuItems = _menuService.GetMenuItems(cardId, categoryId);
            var filterCategories = GetFilterCategories(allCategories, cardId);

            PopulateViewBag(menuItems, allCategories, filterCategories, cardId, categoryId, editId, showCreate);

            return View("~/Views/ManageMenu/Index.cshtml", menuItems);
        }

        // Maakt nieuw menu-item aan met optionele afbeelding
        [HttpPost]
        public async Task<IActionResult> Create(MenuItem item, IFormFile? imageFile, int? cardId, int? categoryId)
        {
            RemoveRetailPriceValidation();
            item.RetailPrice = ParseRetailPrice();
            item.IsActive = true;

            int menuCardId = ParseMenuCardId();

            if (!await ProcessImageUpload(imageFile, item))
                return RedirectToCreate(menuCardId, cardId, categoryId);

            ValidateMenuItemForCreate(item, menuCardId);

            if (!ModelState.IsValid)
                return HandleValidationError(menuCardId, cardId, categoryId);

            return await ExecuteCreate(item, menuCardId, cardId, categoryId);
        }

        // Werkt bestaand menu-item bij
        [HttpPost]
        public async Task<IActionResult> Edit(MenuItem item, IFormFile? imageFile, int? cardId, int? categoryId)
        {
            RemoveRetailPriceValidation();
            item.RetailPrice = ParseRetailPrice();

            var existingItem = GetExistingMenuItem(item.MenuItemID);
            if (existingItem == null)
            {
                SetFlashError(ErrorMessages.MenuItemNotFound);
                return RedirectToAction(nameof(Index));
            }

            item.IsActive = existingItem.IsActive;

            if (!await ProcessImageUpload(imageFile, item, existingItem))
                return RedirectToEdit(item.MenuItemID, cardId, categoryId);

            ValidateMenuItemForEdit(item);

            if (!ModelState.IsValid)
                return HandleEditValidationError(item.MenuItemID, cardId, categoryId);

            return await ExecuteEdit(item, cardId, categoryId);
        }

        // Zet menu-item status aan/uit
        [HttpPost]
        public IActionResult ToggleActive(int id, bool active, int? cardId, int? categoryId)
        {
            try
            {
                _menuService.SetMenuItemActive(id, active);
                var message = active ? "Menu-item geactiveerd." : "Menu-item gedeactiveerd.";
                SetFlashSuccess(message);
            }
            catch
            {
                SetFlashError("Kon status niet aanpassen. Probeer opnieuw.");
            }

            return RedirectToAction(nameof(Index), new { cardId, categoryId });
        }

        // Valideert dat de gekozen categorie hoort bij de geselecteerde kaart
        private (int?, int?) ValidateSelectedCategory(IEnumerable<Category> allCategories, int? cardId, int? categoryId)
        {
            if (!cardId.HasValue || !categoryId.HasValue)
                return (cardId, categoryId);

            var selectedCategory = allCategories.FirstOrDefault(c => c.CategoryID == categoryId.Value);
            if (selectedCategory == null || selectedCategory.MenuCardID != cardId.Value)
                return (cardId, null);

            return (cardId, categoryId);
        }

        // Filtert categorieën op basis van menukaart
        private List<Category> GetFilterCategories(IEnumerable<Category> allCategories, int? cardId)
        {
            return cardId.HasValue
                ? allCategories.Where(c => c.MenuCardID == cardId.Value).ToList()
                : allCategories.ToList();
        }

        // Vult ViewBag met alle benodigde gegevens voor de view
        private void PopulateViewBag(IEnumerable<MenuItem> menuItems, IEnumerable<Category> allCategories,
            IEnumerable<Category> filterCategories, int? cardId, int? categoryId, int? editId, bool showCreate)
        {
            ViewBag.SelectedCardId = cardId;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.AllCategories = allCategories;
            ViewBag.FilterCategories = filterCategories;
            ViewBag.MenuCards = GetMenuCardSelectList();
            ViewBag.ShowCreate = showCreate;
            ViewBag.SelectedMenuCardId = cardId ?? 0;
            ViewBag.Categories = allCategories;

            if (editId.HasValue)
            {
                var editItem = menuItems.FirstOrDefault(m => m.MenuItemID == editId);
                if (editItem != null)
                {
                    ViewBag.EditItem = editItem;
                    ViewBag.IsEdit = true;
                    var itemCategory = allCategories.FirstOrDefault(c => c.CategoryID == editItem.CategoryID);
                    if (itemCategory != null)
                    {
                        ViewBag.SelectedMenuCardId = itemCategory.MenuCardID;
                        ViewBag.Categories = allCategories.Where(c => c.MenuCardID == itemCategory.MenuCardID);
                    }
                }
            }
        }

        private void RemoveRetailPriceValidation()
        {
            ModelState.Remove(nameof(MenuItem.RetailPrice));
            ModelState.Remove("RetailPrice");
        }

        private decimal ParseRetailPrice()
        {
            var rawRetailPrice = Request.Form["RetailPrice"].FirstOrDefault();
            if (!TryParseDecimalFlexible(rawRetailPrice, out var parsedRetailPrice))
            {
                ModelState.AddModelError(nameof(MenuItem.RetailPrice), "Vul een geldige prijs in.");
                return 0m;
            }
            return parsedRetailPrice;
        }

        private int ParseMenuCardId()
        {
            var rawMenuCardId = Request.Form["MenuCardID"].FirstOrDefault();
            return int.TryParse(rawMenuCardId, out var parsed) ? parsed : 0;
        }

        // Verwerkt afbeelding upload of behoudt bestaande afbeelding
        private async Task<bool> ProcessImageUpload(IFormFile? imageFile, MenuItem item, MenuItem? existingItem = null)
        {
            if (imageFile == null)
            {
                if (existingItem != null)
                    item.ImagePath = existingItem.ImagePath;
                return true;
            }

            var (success, path, errorMessage) = await _imageService.UploadImageAsync(imageFile);
            if (!success)
            {
                ModelState.AddModelError("imageFile", errorMessage ?? ErrorMessages.ImageUploadError);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(path))
                item.ImagePath = path;

            return true;
        }

        // Zoekt bestaand menu-item op ID
        private MenuItem? GetExistingMenuItem(int menuItemId)
        {
            return _menuService.GetMenuItems(null, null)
                .FirstOrDefault(m => m.MenuItemID == menuItemId);
        }

        // Voert aanmaken uit met error handling
        private async Task<IActionResult> ExecuteCreate(MenuItem item, int menuCardId, int? cardId, int? categoryId)
        {
            try
            {
                _menuService.AddMenuItem(item);
                SetFlashSuccess($"Menu-item '{item.Name}' is succesvol toegevoegd!");
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
            catch (InvalidOperationException ex)
            {
                SetFlashError(ex.Message);
                return RedirectToCreate(menuCardId, cardId, categoryId);
            }
            catch (ArgumentException ex)
            {
                SetFlashError(ex.Message);
                return RedirectToCreate(menuCardId, cardId, categoryId);
            }
            catch (Exception)
            {
                SetFlashError(ErrorMessages.UnexpectedError);
                return RedirectToCreate(menuCardId, cardId, categoryId);
            }
        }

        private async Task<IActionResult> ExecuteEdit(MenuItem item, int? cardId, int? categoryId)
        {
            try
            {
                _menuService.UpdateMenuItem(item);
                SetFlashSuccess($"Menu-item '{item.Name}' is succesvol bijgewerkt!");
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
            catch (InvalidOperationException ex)
            {
                SetFlashError(ex.Message);
                return RedirectToEdit(item.MenuItemID, cardId, categoryId);
            }
            catch (ArgumentException ex)
            {
                SetFlashError(ex.Message);
                return RedirectToEdit(item.MenuItemID, cardId, categoryId);
            }
            catch (Exception)
            {
                SetFlashError(ErrorMessages.UnexpectedError);
                return RedirectToEdit(item.MenuItemID, cardId, categoryId);
            }
        }

        private IActionResult RedirectToCreate(int menuCardId, int? cardId, int? categoryId) =>
            RedirectToAction(nameof(Index), new { cardId, categoryId, showCreate = true });

        private IActionResult RedirectToEdit(int menuItemId, int? cardId, int? categoryId) =>
            RedirectToAction(nameof(Index), new { editId = menuItemId, cardId, categoryId });

        private IActionResult HandleValidationError(int menuCardId, int? cardId, int? categoryId)
        {
            SetFlashError(GetModelStateErrors());
            return RedirectToCreate(menuCardId, cardId, categoryId);
        }

        private IActionResult HandleEditValidationError(int menuItemId, int? cardId, int? categoryId)
        {
            SetFlashError(GetModelStateErrors());
            return RedirectToEdit(menuItemId, cardId, categoryId);
        }

        // Valideert naam uniekheid bij aanmaken
        private void ValidateMenuItemForCreate(MenuItem item, int menuCardId)
        {
            ValidateItemName(item.Name, isNew: true);
            ValidateItemFields(item);
            ValidateCategoryForCreate(item.CategoryID, menuCardId);
        }

        // Valideert naam uniekheid bij bewerking
        private void ValidateMenuItemForEdit(MenuItem item)
        {
            ValidateItemName(item.Name, isNew: false, existingItemId: item.MenuItemID);
            ValidateItemFields(item);
            ValidateCategoryForEdit(item);
        }

        // Controleert of naam uniek is
        private void ValidateItemName(string name, bool isNew, int? existingItemId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(nameof(MenuItem.Name), "Naam is verplicht.");
                return;
            }

            var items = _menuService.GetMenuItems(null, null);
            bool nameExists = isNew
                ? items.Any(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                : items.Any(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && m.MenuItemID != existingItemId);

            if (nameExists)
            {
                var errorMsg = isNew ? ErrorMessages.MenuItemDuplicateName : ErrorMessages.UpdateMenuItemAlreadyExists;
                ModelState.AddModelError(nameof(MenuItem.Name), errorMsg);
            }
        }

        // Valideert prijs en voorraad velden
        private void ValidateItemFields(MenuItem item)
        {
            if (item.RetailPrice < 0)
                ModelState.AddModelError(nameof(MenuItem.RetailPrice), "Verkoopsprijs kan niet negatief zijn.");

            if (item.Stock < 0)
                ModelState.AddModelError(nameof(MenuItem.Stock), "Voorraad kan niet negatief zijn.");

            if (item.CategoryID <= 0)
                ModelState.AddModelError(nameof(MenuItem.CategoryID), ErrorMessages.InvalidCategory);
        }

        // Valideert of categorie geldig is bij aanmaken
        private void ValidateCategoryForCreate(int categoryId, int menuCardId)
        {
            if (!MenuCardConstants.IsValidMenuCardId(menuCardId))
            {
                ModelState.AddModelError("MenuCardID", ErrorMessages.InvalidMenuCard);
                return;
            }

            if (categoryId <= 0)
                return;

            var category = _categoryService.GetCategories()
                .FirstOrDefault(c => c.CategoryID == categoryId);

            if (category == null)
            {
                ModelState.AddModelError(nameof(MenuItem.CategoryID), ErrorMessages.InvalidCategory);
                return;
            }

            if (category.MenuCardID != menuCardId)
                ModelState.AddModelError(nameof(MenuItem.CategoryID), ErrorMessages.CategoryNotBelongsToCard);
        }

        private void ValidateCategoryForEdit(MenuItem item)
        {
            if (item.CategoryID <= 0)
                return;

            var categories = _categoryService.GetCategories();
            var newCategory = categories.FirstOrDefault(c => c.CategoryID == item.CategoryID);

            if (newCategory == null)
            {
                ModelState.AddModelError(nameof(MenuItem.CategoryID), ErrorMessages.InvalidCategory);
                return;
            }

            var existingItem = GetExistingMenuItem(item.MenuItemID);
            if (existingItem == null)
            {
                ModelState.AddModelError("", ErrorMessages.MenuItemNotFound);
                return;
            }

            var oldCategory = categories.FirstOrDefault(c => c.CategoryID == existingItem.CategoryID);
            if (oldCategory == null)
            {
                ModelState.AddModelError("", "Huidige categorie van dit menu-item bestaat niet meer.");
                return;
            }

            if (newCategory.MenuCardID != oldCategory.MenuCardID)
                ModelState.AddModelError(nameof(MenuItem.CategoryID), ErrorMessages.CanOnlyChangeCategoryWithinCard);
        }

        // Retourneert dropdown lijst van menukaarten
        private static List<SelectListItem> GetMenuCardSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = MenuCardConstants.LunchCardId.ToString(), Text = MenuCardConstants.LunchCardName },
                new SelectListItem { Value = MenuCardConstants.DinnerCardId.ToString(), Text = MenuCardConstants.DinnerCardName },
                new SelectListItem { Value = MenuCardConstants.DrinksCardId.ToString(), Text = MenuCardConstants.DrinksCardName }
            };
        }

        // Parse prijzen flexibel (komma of punt als decimaal scheidingsteken)
        private static bool TryParseDecimalFlexible(string? input, out decimal value)
        {
            value = 0m;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            var trimmed = input.Trim();
            bool hasDot = trimmed.Contains('.');
            bool hasComma = trimmed.Contains(',');

            if (hasDot && !hasComma)
                return decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

            if (hasComma && !hasDot)
                return decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.GetCultureInfo("nl-NL"), out value);

            if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.GetCultureInfo("nl-NL"), out value))
                return true;

            if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.GetCultureInfo("en-US"), out value))
                return true;

            string normalized = trimmed.Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        // Verzamelt alle validatie fouten
        private string GetModelStateErrors()
        {
            return string.Join("\n", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .Where(msg => !msg.Contains("The value '' is invalid.", StringComparison.OrdinalIgnoreCase))
                .Distinct());
        }

        // Helpers voor meldingen
        private void SetFlashError(string message) => TempData[FlashMessages.ErrorKey] = message;
        private void SetFlashSuccess(string message) => TempData[FlashMessages.SuccessKey] = message;
    }
}