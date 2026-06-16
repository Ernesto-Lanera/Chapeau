using Microsoft.AspNetCore.Http;
using System.Globalization;
using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ICategoryService _categoryService;
        private readonly IImageService _imageService;
        private readonly ILogger<MenuService> _logger;

        public MenuService(
            IMenuRepository menuRepository,
            ICategoryService categoryService,
            IImageService imageService,
            ILogger<MenuService> logger)
        {
            _menuRepository = menuRepository;
            _categoryService = categoryService;
            _imageService = imageService;
            _logger = logger;
        }

        public MenuManagementViewModel GetManagementOverview(
            int? cardId, int? categoryId, int? editId, bool showCreate)
        {
            // Eerst maken we de filter veilig: categorie en kaart moeten bij elkaar horen.
            int? selectedCategoryId = _categoryService.GetValidCategoryId(cardId, categoryId);
            MenuItem? editItem = editId.HasValue ? _menuRepository.GetMenuItemById(editId.Value) : null;

            List<Category> allCategories = _categoryService.GetCategories();
            List<Category> filterCategories = _categoryService.GetCategoriesByCard(cardId);
            List<Category> formCategories = GetFormCategories(editItem, cardId, allCategories);

            return new MenuManagementViewModel
            {
                MenuItems = _menuRepository.GetMenuItems(cardId, selectedCategoryId),
                MenuCards = _categoryService.GetMenuCards(),
                FilterCategories = filterCategories,
                FormCategories = formCategories,
                SelectedCardId = cardId,
                SelectedCategoryId = selectedCategoryId,
                ShowCreate = showCreate,
                EditItem = editItem
            };
        }

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            return _menuRepository.GetMenuItems(cardId, categoryId);
        }

        public MenuItem? GetMenuItemById(int menuItemId)
        {
            ValidateId(menuItemId);
            return _menuRepository.GetMenuItemById(menuItemId);
        }

        public async Task AddMenuItemAsync(MenuItemInputModel input, IFormFile? imageFile)
        {
            Category category = ValidateForCreate(input);
            MenuItem item = CreateItem(input, category);
            item.IsActive = true;
            item.ImagePath = await GetUploadedImagePath(imageFile, string.Empty);

            _menuRepository.AddMenuItem(item);
            _logger.LogInformation("Menu-item aangemaakt: {MenuItemId}.", item.MenuItemID);
        }

        public async Task UpdateMenuItemAsync(MenuItemInputModel input, IFormFile? imageFile)
        {
            ValidateId(input.MenuItemID);
            MenuItem existingItem = _menuRepository.GetMenuItemById(input.MenuItemID)
                ?? throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);

            Category category = ValidateForEdit(input, existingItem);
            MenuItem updatedItem = CreateItem(input, category);
            updatedItem.MenuItemID = existingItem.MenuItemID;
            updatedItem.IsActive = existingItem.IsActive;
            updatedItem.ImagePath = await GetUploadedImagePath(imageFile, existingItem.ImagePath);

            _menuRepository.UpdateMenuItem(updatedItem);
            _logger.LogInformation("Menu-item gewijzigd: {MenuItemId}.", updatedItem.MenuItemID);
        }

        public void SetMenuItemActive(int menuItemId, bool active)
        {
            ValidateId(menuItemId);
            _menuRepository.SetMenuItemActive(menuItemId, active);
            _logger.LogInformation("Menu-itemstatus gewijzigd: {MenuItemId} - {Active}.", menuItemId, active);
        }

        private List<Category> GetFormCategories(MenuItem? editItem, int? cardId, List<Category> allCategories)
        {
            if (editItem != null)
            {
                return _categoryService.GetCategoriesByCard(editItem.Category.MenuCardID);
            }

            // Bij een nieuw item staan alle categorieën in de HTML.
            // JavaScript laat daarna alleen zien wat bij de gekozen kaart hoort.
            return allCategories;
        }

        private Category ValidateForCreate(MenuItemInputModel input)
        {
            // Bij toevoegen controleren we eerst de invoer, daarna slaan we het item pas op.
            ValidateInput(input);
            if (!MenuCardConstants.IsValidMenuCardId(input.MenuCardID))
            {
                throw new ArgumentException(ErrorMessages.InvalidMenuCard);
            }

            Category category = ValidateCategoryMatchesCard(input.CategoryID, input.MenuCardID);
            ValidateUniqueName(input.Name, null);
            return category;
        }

        private Category ValidateForEdit(MenuItemInputModel input, MenuItem existingItem)
        {
            ValidateInput(input);
            Category category = ValidateCategoryMatchesCard(input.CategoryID, existingItem.Category.MenuCardID);
            ValidateUniqueName(input.Name, input.MenuItemID);
            return category;
        }

        private static void ValidateInput(MenuItemInputModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new ArgumentException(ErrorMessages.MenuItemNameRequired);
            }

            if (input.Name.Length > 100)
            {
                throw new ArgumentException(ErrorMessages.MenuItemNameTooLong);
            }

            if (input.Stock < 0)
            {
                throw new ArgumentException(ErrorMessages.MenuItemStockNegative);
            }

            ParsePrice(input.RetailPrice);
        }

        private Category ValidateCategoryMatchesCard(int categoryId, int expectedCardId)
        {
            Category? category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
            {
                throw new ArgumentException(ErrorMessages.InvalidCategory);
            }

            if (category.MenuCardID != expectedCardId)
            {
                throw new ArgumentException(ErrorMessages.CategoryNotBelongsToCard);
            }

            return category;
        }

        private void ValidateUniqueName(string name, int? excludedMenuItemId)
        {
            if (_menuRepository.NameExists(name, excludedMenuItemId))
            {
                if (excludedMenuItemId.HasValue)
                {
                    throw new InvalidOperationException(ErrorMessages.UpdateMenuItemAlreadyExists);
                }

                throw new InvalidOperationException(ErrorMessages.MenuItemDuplicateName);
            }
        }

        private static MenuItem CreateItem(MenuItemInputModel input, Category category)
        {
            // Van de formuliergegevens maken we één MenuItem object voor de repository.
            return new MenuItem
            {
                Name = input.Name,
                RetailPrice = ParsePrice(input.RetailPrice),
                Stock = input.Stock,
                CategoryID = category.CategoryID,
                IsAlcoholic = category.AllowsAlcoholicChoice && input.IsAlcoholic
            };
        }

        private async Task<string> GetUploadedImagePath(IFormFile? imageFile, string existingPath)
        {
            (bool success, string? path, string? errorMessage) = await _imageService.UploadImageAsync(imageFile);
            if (!success)
            {
                throw new ArgumentException(errorMessage ?? ErrorMessages.ImageUploadError);
            }

            return path ?? existingPath;
        }

        private static decimal ParsePrice(string priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText))
            {
                throw new ArgumentException(ErrorMessages.MenuItemPriceRequired);
            }

            string normalized = priceText.Replace(',', '.');
            decimal price;
            bool validPrice = decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out price);

            if (!validPrice || price <= 0)
            {
                throw new ArgumentException(ErrorMessages.MenuItemPriceInvalid);
            }

            return price;
        }

        private static void ValidateId(int menuItemId)
        {
            if (menuItemId <= 0)
            {
                throw new ArgumentException("Ongeldig menu-item ID.", nameof(menuItemId));
            }
        }
    }
}
