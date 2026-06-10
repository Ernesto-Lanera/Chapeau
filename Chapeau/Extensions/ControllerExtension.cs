using Chapeau.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Chapeau.Extensions
{
    public static class ControllerExtension
    {
        public static void ShowNotification(this Controller controller, string message, string type = "success")
        {
            var notification = new Notification { Message = message, Type = type };
            controller.TempData["GlobalNotification"] = JsonSerializer.Serialize(notification);
        }
    }
}
