using Chapeau.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Chapeau.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            if (TempData["GlobalNotification"] != null)
            {
                var json = TempData["GlobalNotification"]!.ToString();
                var notification = JsonSerializer.Deserialize<Notification>(json);
                return View(notification);
            }

            return Content(string.Empty);
        }
    }
}
