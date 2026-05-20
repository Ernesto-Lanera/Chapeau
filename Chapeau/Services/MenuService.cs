using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories.Menu;
using Microsoft.Extensions.Logging;

namespace Chapeau.Services
{
    public class MenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ILogger<MenuService> _logger;

        public MenuService(IMenuRepository menuRepository, ILogger<MenuService> logger)
        {
            _menuRepository = menuRepository;
            _logger = logger;
        }

        // Haalt menu-items op gefilterd op kaart en categorie
        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            return _menuRepository.GetMenuItems(cardId, categoryId);
        }

        // Haalt specifiek menu-item op via ID
        public MenuItem? GetMenuItemById(int menuItemId)
        {
            if (menuItemId <= 0)
            {
                throw new ArgumentException("Ongeldig menu item.");
            }

            return _menuRepository.GetMenuItemById(menuItemId);
        }

        // Voegt nieuw menu-item toe met standaard instellingen
        public void AddMenuItem(MenuItem menuItem)
        {
            ValidateMenuItem(menuItem, isEdit: false);
            menuItem.IsActive = true;
            _menuRepository.AddMenuItem(menuItem);
            _logger.LogInformation("Menu item added: {MenuItemName}", menuItem.Name);
        }

        // Werkt bestaand menu-item bij
        public void UpdateMenuItem(MenuItem menuItem)
        {
            ValidateMenuItem(menuItem, isEdit: true);
            _menuRepository.UpdateMenuItem(menuItem);
            _logger.LogInformation("Menu item updated: {MenuItemId}", menuItem.MenuItemID);
        }

        // Zet menu-item actief of inactief
        public void SetMenuItemActive(int menuItemId, bool active)
        {
            if (menuItemId <= 0)
            {
                throw new ArgumentException("Ongeldig menu item.");
            }

            _menuRepository.SetMenuItemActive(menuItemId, active);
            _logger.LogInformation("Menu item {MenuItemId} status changed to {Active}", menuItemId, active);
        }

        // Werkt voorraad bij
        public void ChangeStock(int menuItemId, int stock)
        {
            if (menuItemId <= 0)
            {
                throw new ArgumentException("Ongeldig menu item.");
            }

            if (stock < 0)
            {
                throw new ArgumentException("Voorraad mag niet negatief zijn.");
            }

            _menuRepository.ChangeStock(menuItemId, stock);
            _logger.LogInformation("Stock changed for menu item {MenuItemId} to {Stock}", menuItemId, stock);
        }

        // Valideert menu-item fields
        private void ValidateMenuItem(MenuItem menuItem, bool isEdit)
        {
            if (menuItem == null)
            {
                throw new ArgumentException("Menu item ontbreekt.");
            }

            if (isEdit && menuItem.MenuItemID <= 0)
            {
                throw new ArgumentException("Ongeldig menu item.");
            }

            if (string.IsNullOrWhiteSpace(menuItem.Name))
            {
                throw new ArgumentException("Naam is verplicht.");
            }

            if (menuItem.RetailPrice <= 0)
            {
                throw new ArgumentException("Prijs moet hoger zijn dan 0.");
            }

            if (menuItem.Stock < 0)
            {
                throw new ArgumentException("Voorraad mag niet negatief zijn.");
            }

            if (menuItem.CategoryID <= 0)
            {
                throw new ArgumentException("Kies een geldige categorie.");
            }
        }
    }
}