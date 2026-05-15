using System.Collections.Generic;
using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories.Menu;
using Microsoft.Extensions.Logging;

namespace Chapeau.Services
{
    /// Service for menu-related business logic.
    public class MenuService(IMenuRepository menuRepository, ILogger<MenuService> logger)
    {
        private readonly IMenuRepository _menuRepository = menuRepository;
        private readonly ILogger<MenuService> _logger = logger;

        /// Gets all menu items, optionally filtered by category.
        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            return _menuRepository.GetMenuItems(cardId, categoryId);
        }

        /// Gets only active menu items.
        public List<MenuItem> GetActiveMenuItems(int? categoryId = null)
        {
            var items = _menuRepository.GetMenuItems(null, categoryId);
            return items.Where(x => x.IsActive).ToList();
        }

        /// Gets the average price of all menu items.
        public decimal GetAverageMenuItemPrice()
        {
            var items = _menuRepository.GetMenuItems(null, null);
            if (!items.Any())
                return 0m;

            return items.Average(x => x.RetailPrice);
        }

        /// Gets low stock items (less than 10 units).
        public List<MenuItem> GetLowStockItems()
        {
            const int lowStockThreshold = 10;
            var items = _menuRepository.GetMenuItems(null, null);
            return items.Where(x => x.Stock < lowStockThreshold && x.IsActive).ToList();
        }

        /// Adds a menu item with validation.
        public void AddMenuItem(MenuItem item)
        {
            ValidateMenuItem(item);
            _menuRepository.AddMenuItem(item);
            _logger.LogInformation("Menu item added successfully: {ItemName}", item.Name);
        }

        /// Updates a menu item with validation.
        public void UpdateMenuItem(MenuItem item)
        {
            ValidateMenuItem(item);
            _menuRepository.UpdateMenuItem(item);
            _logger.LogInformation("Menu item updated successfully: {ItemId}", item.MenuItemID);
        }

        public void SetMenuItemActive(int id, bool active)
        {
            _menuRepository.SetMenuItemActive(id, active);
        }

        public void ChangeStock(int id, int newStock)
        {
            if (newStock < 0)
                throw new ArgumentException("Stock cannot be negative");

            _menuRepository.ChangeStock(id, newStock);
            _logger.LogInformation("Stock changed for item {ItemId}: {NewStock}", id, newStock);
        }

        private void ValidateMenuItem(MenuItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new ArgumentException("Menu item name is required");

            if (item.RetailPrice < 0)
                throw new ArgumentException("Retail price cannot be negative");

            if (item.Stock < 0)
                throw new ArgumentException("Menu item stock cannot be negative");
        }
    }
}
