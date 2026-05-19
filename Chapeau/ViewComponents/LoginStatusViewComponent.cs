using Microsoft.AspNetCore.Mvc;

namespace Chapeau.ViewComponents
{
    public class LoginStatusViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
