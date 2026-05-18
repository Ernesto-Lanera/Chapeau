using Microsoft.AspNetCore.Mvc;
using Chapeau.Models;

namespace Chapeau.Components.StockTable;

public class StockTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IEnumerable<MenuItem> menuItems,
        IEnumerable<Category> categories,
        int? selectedCardId,
        int? selectedCategoryId)
    {
        var model = new StockTableModel
        {
            MenuItems = menuItems ?? [],
            Categories = categories ?? [],
            SelectedCardId = selectedCardId,
            SelectedCategoryId = selectedCategoryId
        };
        return View(model);
    }
}

public class StockTableModel
{
    public IEnumerable<MenuItem> MenuItems { get; set; } = [];
    public IEnumerable<Category> Categories { get; set; } = [];
    public int? SelectedCardId { get; set; }
    public int? SelectedCategoryId { get; set; }
}
