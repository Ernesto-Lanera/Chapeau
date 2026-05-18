using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Components.StockHeader;

public class StockHeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
