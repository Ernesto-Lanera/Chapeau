using Chapeau.Services.Overview;
using Chapeau.ViewModels;
using Chapeau.ViewModels.Overview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Chapeau.Controllers
{
    public class HomeController(IEmployeeService employeeService) : Controller
    {
        private readonly IEmployeeService _employeeService = employeeService;

        public IActionResult Index()
        {
            bool isConnected = _employeeService.TestConnection();

            var viewModel = new HomeViewModel
            {
                IsDatabaseConnected = isConnected,
                DatabaseStatus = isConnected
                    ? "Verbonden met de database."
                    : "Niet verbonden met de database: Controleer de verbindingsreeks."
            };

            return View(viewModel);
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
