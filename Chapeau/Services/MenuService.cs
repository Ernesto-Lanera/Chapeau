using System.Collections.Generic;
using Chapeau.Models;
using Chapeau.Repositories;

namespace Chapeau.Services
{
    public class MenuService
    {
        private readonly MenuRepository _menuRepository;

        public MenuService(MenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            return _menuRepository.GetMenuItems(cardId, categoryId);
        }

        public void AddMenuItem(MenuItem item)
        {
            _menuRepository.AddMenuItem(item);
        }

        public void UpdateMenuItem(MenuItem item)
        {
            _menuRepository.UpdateMenuItem(item);
        }

        public void SetMenuItemActive(int id, bool active)
        {
            _menuRepository.SetMenuItemActive(id, active);
        }

        public void ChangeStock(int id, int newStock)
        {
            _menuRepository.ChangeStock(id, newStock);
        }
    }
}
