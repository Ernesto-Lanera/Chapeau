using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chapeau.Models;

namespace Chapeau.Components.ManageMenuCreateForm;

public class ManageMenuCreateFormViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IEnumerable<SelectListItem> menuCards,
        IEnumerable<Category> formCategories,
        int? selectedCardId,
        int? selectedCategoryId)
    {
        var model = new ManageMenuCreateFormModel
        {
            MenuCards = menuCards ?? [],
            FormCategories = formCategories ?? [],
            SelectedCardId = selectedCardId,
            SelectedCategoryId = selectedCategoryId
        };
        return View(model);
    }
}

public class ManageMenuCreateFormModel
{
    public IEnumerable<SelectListItem> MenuCards { get; set; } = [];
    public IEnumerable<Category> FormCategories { get; set; } = [];
    public int? SelectedCardId { get; set; }
    public int? SelectedCategoryId { get; set; }
}
