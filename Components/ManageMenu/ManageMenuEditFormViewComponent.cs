using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chapeau.Models;

namespace Chapeau.Components.ManageMenuEditForm;

public class ManageMenuEditFormViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        MenuItem editItem,
        IEnumerable<SelectListItem> menuCards,
        IEnumerable<Category> formCategories,
        int? selectedMenuCardId,
        int? selectedCardId,
        int? selectedCategoryId)
    {
        var model = new ManageMenuEditFormModel
        {
            EditItem = editItem,
            MenuCards = menuCards ?? [],
            FormCategories = formCategories ?? [],
            SelectedMenuCardId = selectedMenuCardId ?? 0,
            SelectedCardId = selectedCardId,
            SelectedCategoryId = selectedCategoryId
        };
        return View(model);
    }
}

public class ManageMenuEditFormModel
{
    public MenuItem EditItem { get; set; }
    public IEnumerable<SelectListItem> MenuCards { get; set; } = [];
    public IEnumerable<Category> FormCategories { get; set; } = [];
    public int SelectedMenuCardId { get; set; }
    public int? SelectedCardId { get; set; }
    public int? SelectedCategoryId { get; set; }
}
