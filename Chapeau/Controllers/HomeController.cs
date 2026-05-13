using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Chapeau.Controllers
{
    public class HomeController(IConfiguration configuration, EmployeeService employeeService) : Controller
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly EmployeeService _employeeService = employeeService;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

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
