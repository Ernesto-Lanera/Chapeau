using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chapeau.Models;

namespace Chapeau.Components.ManageMenuFilter;

public class ManageMenuFilterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IEnumerable<SelectListItem> menuCards,
        IEnumerable<Category> filterCategories,
        int? selectedCardId,
        int? selectedCategoryId)
    {
        var model = new ManageMenuFilterModel
        {
            MenuCards = menuCards ?? [],
            FilterCategories = filterCategories ?? [],
            SelectedCardId = selectedCardId,
            SelectedCategoryId = selectedCategoryId
        };
        return View(model);
    }
}

public class ManageMenuFilterModel
{
    public IEnumerable<SelectListItem> MenuCards { get; set; } = [];
    public IEnumerable<Category> FilterCategories { get; set; } = [];
    public int? SelectedCardId { get; set; }
    public int? SelectedCategoryId { get; set; }
}
