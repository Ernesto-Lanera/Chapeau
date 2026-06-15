using Microsoft.AspNetCore.Mvc;

namespace Chapeau.ViewComponents
{
    /// <summary>
    /// View component that renders the login/logout status in the navigation bar.
    /// Shows the current user's name and logout button when authenticated, or a login link otherwise.
    /// </summary>
    public class LoginStatusViewComponent : ViewComponent
    {
        /// <summary>Renders the login status partial view.</summary>
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
