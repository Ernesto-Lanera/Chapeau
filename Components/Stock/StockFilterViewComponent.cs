using Microsoft.AspNetCore.Mvc;
using Chapeau.Models;

namespace Chapeau.Components.StockFilter;

public class StockFilterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IEnumerable<Category> categories,
        int? selectedCardId,
        int? selectedCategoryId)
    {
        var model = new StockFilterModel
        {
            Categories = categories ?? [],
            SelectedCardId = selectedCardId,
            SelectedCategoryId = selectedCategoryId
        };
        return View(model);
    }
}

public class StockFilterModel
{
    public IEnumerable<Category> Categories { get; set; } = [];
    public int? SelectedCardId { get; set; }
    public int? SelectedCategoryId { get; set; }
}
