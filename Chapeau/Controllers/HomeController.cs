using Chapeau.Constants;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Chapeau.Controllers
{
    public class HomeController(EmployeeService employeeService) : Controller
    {
        private readonly EmployeeService _employeeService = employeeService;

        public IActionResult Index()
        {
            string dbStatus = GetDatabaseStatus();

            ViewBag.DbStatus = dbStatus;
            return View();
        }

        private string GetDatabaseStatus()
        {
            bool isConnected = _employeeService.TestConnection();

            return isConnected
                ? AuthConstants.DatabaseConnected
                : AuthConstants.DatabaseNotConnectedPrefix + "Controleer de verbindingsreeks.";
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var errorMsg = exceptionHandlerPathFeature?.Error?.Message;

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = errorMsg
            });
        }
    }
}
