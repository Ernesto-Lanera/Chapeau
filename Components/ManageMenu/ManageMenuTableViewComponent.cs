using Microsoft.AspNetCore.Mvc;
using Chapeau.Models;

namespace Chapeau.Components.ManageMenuTable;

public class ManageMenuTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IEnumerable<MenuItem> menuItems,
        IEnumerable<Category> allCategories,
        MenuItem? editItem,
        int? selectedCardId,
        int? selectedCategoryId)
    {
        var model = new ManageMenuTableModel
        {
            MenuItems = menuItems ?? [],
            AllCategories = allCategories ?? [],
            EditItem = editItem,
            SelectedCardId = selectedCardId,
            SelectedCategoryId = selectedCategoryId
        };
        return View(model);
    }
}

public class ManageMenuTableModel
{
    public IEnumerable<MenuItem> MenuItems { get; set; } = [];
    public IEnumerable<Category> AllCategories { get; set; } = [];
    public MenuItem? EditItem { get; set; }
    public int? SelectedCardId { get; set; }
    public int? SelectedCategoryId { get; set; }
}
