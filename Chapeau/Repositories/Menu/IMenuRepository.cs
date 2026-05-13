using System.Collections.Generic;
using Chapeau.Models;

namespace Chapeau.Repositories.Menu
{
    public interface IMenuRepository
    {
        List<MenuItem> GetMenuItems(int? cardId, int? categoryId);
        void AddMenuItem(MenuItem item);
        void UpdateMenuItem(MenuItem item);
        void SetMenuItemActive(int id, bool active);
        void ChangeStock(int id, int newStock);
    }
}