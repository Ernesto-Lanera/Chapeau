using Chapeau.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Chapeau.Extensions
{
    /// <summary>
    /// Extension methods for ASP.NET Core Controllers to provide common UI helpers.
    /// </summary>
    public static class ControllerExtension
    {
        /// <summary>
        /// Stores a toast notification in TempData that will be displayed on the next page render.
        /// </summary>
        public static void ShowNotification(this Controller controller, string message, string type = "success")
        {
            var notification = new Notification { Message = message, Type = type };
            controller.TempData["GlobalNotification"] = JsonSerializer.Serialize(notification);
        }
    }
}
