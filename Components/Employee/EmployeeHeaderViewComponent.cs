using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Components.EmployeeHeader;

public class EmployeeHeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(bool showCreate)
    {
        return View(showCreate);
    }
}
