using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public class StockService(
        IMenuRepository menuRepository,
        ICategoryRepository categoryRepository,
        ILogger<StockService> logger) : IStockService
    {
        private readonly IMenuRepository _menuRepository = menuRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly ILogger<StockService> _logger = logger;

        public StockOverviewViewModel GetOverview(int? cardId, int? categoryId)
        {
            List<Category> categories = _categoryRepository.GetCategories();
            categoryId = ValidateCategoryFilter(cardId, categoryId, categories);

            return new StockOverviewViewModel
            {
                MenuItems = _menuRepository.GetMenuItems(cardId, categoryId),
                Categories = FilterCategories(categories, cardId),
                MenuCards = CreateMenuCards(),
                SelectedCardId = cardId,
                SelectedCategoryId = categoryId
            };
        }

        public MenuItem ChangeStock(int menuItemId, int newStock)
        {
            MenuItem item = _menuRepository.GetMenuItemById(menuItemId)
                ?? throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);

            if (!item.CanChangeStock)
            {
                throw new InvalidOperationException(ErrorMessages.InactiveMenuItemStockChangeNotAllowed);
            }

            item.ChangeStock(newStock);
            _menuRepository.ChangeStock(item.MenuItemID, item.Stock);
            _logger.LogInformation("Voorraad gewijzigd: {MenuItemId} naar {Stock}.", item.MenuItemID, item.Stock);
            return item;
        }

        private static int? ValidateCategoryFilter(int? cardId, int? categoryId, IEnumerable<Category> categories)
        {
            if (!categoryId.HasValue) return null;
            Category? category = categories.FirstOrDefault(item => item.CategoryID == categoryId.Value);
            if (category is null) return null;
            return cardId.HasValue && category.MenuCardID != cardId.Value ? null : categoryId;
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
    }
}
