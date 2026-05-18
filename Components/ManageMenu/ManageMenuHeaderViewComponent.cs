using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Components.ManageMenuHeader;

public class ManageMenuHeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(int? selectedCardId, int? selectedCategoryId, bool showCreate)
    {
        var model = new ManageMenuHeaderModel
        {
            SelectedCardId = selectedCardId,
            SelectedCategoryId = selectedCategoryId,
            ShowCreate = showCreate
        };
        return View(model);
    }
}

public class ManageMenuHeaderModel
{
    public int? SelectedCardId { get; set; }
    public int? SelectedCategoryId { get; set; }
    public bool ShowCreate { get; set; }
}
