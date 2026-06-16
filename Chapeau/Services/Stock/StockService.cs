using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public class StockService : IStockService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<StockService> _logger;

        public StockService(
            IMenuRepository menuRepository,
            ICategoryService categoryService,
            ILogger<StockService> logger)
        {
            _menuRepository = menuRepository;
            _categoryService = categoryService;
            _logger = logger;
        }

        public StockOverviewViewModel GetOverview(int? cardId, int? categoryId)
        {
            // Voorraad gebruikt dezelfde categoriefilter als menu beheer.
            int? selectedCategoryId = _categoryService.GetValidCategoryId(cardId, categoryId);

            return new StockOverviewViewModel
            {
                MenuItems = _menuRepository.GetMenuItems(cardId, selectedCategoryId),
                Categories = _categoryService.GetCategoriesByCard(cardId),
                MenuCards = _categoryService.GetMenuCards(),
                SelectedCardId = cardId,
                SelectedCategoryId = selectedCategoryId
            };
        }

        public MenuItem ChangeStock(int menuItemId, int newStock)
        {
            MenuItem? item = _menuRepository.GetMenuItemById(menuItemId);
            if (item == null)
            {
                throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);
            }

            if (!item.CanChangeStock)
            {
                throw new InvalidOperationException(ErrorMessages.InactiveMenuItemStockChangeNotAllowed);
            }

            // Het MenuItem controleert of de voorraad geldig is. Daarna slaan we hem op.
            item.ChangeStock(newStock);
            _menuRepository.ChangeStock(item.MenuItemID, item.Stock);
            _logger.LogInformation("Voorraad gewijzigd: {MenuItemId} naar {Stock}.", item.MenuItemID, item.Stock);
            return item;
        }
    }
}
