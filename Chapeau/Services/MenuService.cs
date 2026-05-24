using Microsoft.AspNetCore.Http;
using System.Globalization;
using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories.Category;
using Chapeau.Repositories.Menu;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public class MenuService(
        IMenuRepository menuRepository,
        ICategoryRepository categoryRepository,
        IImageService imageService,
        ILogger<MenuService> logger) : IMenuService
    {
        private readonly IMenuRepository _menuRepository = menuRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IImageService _imageService = imageService;
        private readonly ILogger<MenuService> _logger = logger;

        public MenuManagementViewModel GetManagementOverview(
            int? cardId, int? categoryId, int? editId, bool showCreate)
        {
            List<Category> categories = _categoryRepository.GetCategories();
            categoryId = ValidateFilterCategory(cardId, categoryId, categories);
            MenuItem? editItem = editId.HasValue ? _menuRepository.GetMenuItemById(editId.Value) : null;
            int? formCardId = editItem?.Category.MenuCardID ?? cardId;

            return new MenuManagementViewModel
            {
                MenuItems = _menuRepository.GetMenuItems(cardId, categoryId),
                MenuCards = CreateMenuCards(),
                FilterCategories = FilterCategories(categories, cardId),
                FormCategories = FilterCategories(categories, formCardId),
                SelectedCardId = cardId,
                SelectedCategoryId = categoryId,
                ShowCreate = showCreate,
                EditItem = editItem
            };
        }

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId) =>
            _menuRepository.GetMenuItems(cardId, categoryId);

        public MenuItem? GetMenuItemById(int menuItemId)
        {
            ValidateId(menuItemId);
            return _menuRepository.GetMenuItemById(menuItemId);
        }

        public async Task AddMenuItemAsync(MenuItemInputModel input, IFormFile? imageFile)
        {
            ValidateForCreate(input);
            var item = CreateItem(input);
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

            ValidateForEdit(input, existingItem);
            MenuItem updatedItem = CreateItem(input);
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

        private void ValidateForCreate(MenuItemInputModel input)
        {
            ValidateInput(input);
            if (!MenuCardConstants.IsValidMenuCardId(input.MenuCardID))
            {
                throw new ArgumentException(ErrorMessages.InvalidMenuCard);
            }

            ValidateCategoryMatchesCard(input.CategoryID, input.MenuCardID);
            ValidateUniqueName(input.Name, null);
        }

        private void ValidateForEdit(MenuItemInputModel input, MenuItem existingItem)
        {
            ValidateInput(input);
            ValidateCategoryMatchesCard(input.CategoryID, existingItem.Category.MenuCardID);
            ValidateUniqueName(input.Name, input.MenuItemID);
        }

        private static void ValidateInput(MenuItemInputModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new ArgumentException("Naam is verplicht.");
            }

            if (input.Name.Trim().Length > 100)
            {
                throw new ArgumentException("Naam mag niet langer zijn dan 100 karakters.");
            }

            if (input.Stock < 0)
            {
                throw new ArgumentException("Voorraad mag niet negatief zijn.");
            }

            ParsePrice(input.RetailPrice);
        }

        private void ValidateCategoryMatchesCard(int categoryId, int expectedCardId)
        {
            Category category = _categoryRepository.GetCategoryById(categoryId)
                ?? throw new ArgumentException(ErrorMessages.InvalidCategory);

            if (category.MenuCardID != expectedCardId)
            {
                throw new ArgumentException(ErrorMessages.CategoryNotBelongsToCard);
            }
        }

        private void ValidateUniqueName(string name, int? excludedMenuItemId)
        {
            if (_menuRepository.NameExists(name.Trim(), excludedMenuItemId))
            {
                throw new InvalidOperationException(
                    excludedMenuItemId.HasValue
                        ? ErrorMessages.UpdateMenuItemAlreadyExists
                        : ErrorMessages.MenuItemDuplicateName);
            }
        }

        private static MenuItem CreateItem(MenuItemInputModel input) => new()
        {
            Name = input.Name.Trim(),
            RetailPrice = ParsePrice(input.RetailPrice),
            Stock = input.Stock,
            CategoryID = input.CategoryID,
            IsAlcoholic = input.IsAlcoholic
        };

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
                throw new ArgumentException("Prijs is verplicht.");
            }

            string normalized = priceText.Trim().Replace(',', '.');
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price)
                || price <= 0)
            {
                throw new ArgumentException("Prijs moet hoger zijn dan 0.");
            }

            return price;
        }

        private static int? ValidateFilterCategory(int? cardId, int? categoryId, IEnumerable<Category> categories)
        {
            if (!categoryId.HasValue) return null;
            Category? selected = categories.FirstOrDefault(category => category.CategoryID == categoryId.Value);
            if (selected is null) return null;
            if (cardId.HasValue && selected.MenuCardID != cardId.Value) return null;
            return categoryId;
        }

        private static List<Category> FilterCategories(IEnumerable<Category> categories, int? cardId) =>
            cardId.HasValue
                ? categories.Where(category => category.MenuCardID == cardId.Value).ToList()
                : categories.ToList();

        private static List<MenuCard> CreateMenuCards() =>
        [
            new() { MenuCardID = MenuCardConstants.LunchCardId, Name = MenuCardConstants.LunchCardName },
            new() { MenuCardID = MenuCardConstants.DinnerCardId, Name = MenuCardConstants.DinnerCardName },
            new() { MenuCardID = MenuCardConstants.DrinksCardId, Name = MenuCardConstants.DrinksCardName }
        ];

        private static void ValidateId(int menuItemId)
        {
            if (menuItemId <= 0)
            {
                throw new ArgumentException("Ongeldig menu-item.");
            }
        }
    }
}
