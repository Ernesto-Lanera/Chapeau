using Microsoft.AspNetCore.Http;
using Chapeau.Models;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public interface IMenuService
    {
        MenuManagementViewModel GetManagementOverview(int? cardId, int? categoryId, int? editId, bool showCreate);
        List<MenuItem> GetMenuItems(int? cardId, int? categoryId);
        MenuItem? GetMenuItemById(int menuItemId);
        Task AddMenuItemAsync(MenuItemInputModel input, IFormFile? imageFile);
        Task UpdateMenuItemAsync(MenuItemInputModel input, IFormFile? imageFile);
        void SetMenuItemActive(int menuItemId, bool active);
    }
}
