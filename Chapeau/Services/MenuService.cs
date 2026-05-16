using Chapeau.Models;
using Chapeau.Repositories.Menu;

namespace Chapeau.Services
{
    public class MenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            return _menuRepository.GetMenuItems(cardId, categoryId);
        }

        public MenuItem? GetMenuItemById(int menuItemId)
        {
            if (menuItemId <= 0)
            {
                throw new ArgumentException("Ongeldig menu item.");
            }

            return _menuRepository.GetMenuItemById(menuItemId);
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            ValidateMenuItem(menuItem, isEdit: false);

            menuItem.IsActive = true;

            _menuRepository.AddMenuItem(menuItem);
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            ValidateMenuItem(menuItem, isEdit: true);

            _menuRepository.UpdateMenuItem(menuItem);
        }

        public void SetMenuItemActive(int menuItemId, bool active)
        {
            if (menuItemId <= 0)
            {
                throw new ArgumentException("Ongeldig menu item.");
            }

            _menuRepository.SetMenuItemActive(menuItemId, active);
        }

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
        }

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