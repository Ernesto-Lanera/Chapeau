using Chapeau.Models;

namespace Chapeau.Repositories.Menu
{
    public interface IMenuRepository
    {
        List<MenuItem> GetMenuItems(int? cardId, int? categoryId);
        MenuItem? GetMenuItemById(int menuItemId);
        bool NameExists(string name, int? excludedMenuItemId = null);
        void AddMenuItem(MenuItem item);
        void UpdateMenuItem(MenuItem item);
        void SetMenuItemActive(int menuItemId, bool active);
        void ChangeStock(int menuItemId, int newStock);
    }
}
