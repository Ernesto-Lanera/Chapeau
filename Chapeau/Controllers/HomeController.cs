using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Chapeau.Controllers
{
    public class HomeController(IConfiguration configuration, EmployeeService employeeService) : Controller
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly EmployeeService _employeeService = employeeService;

        public IActionResult Index()
        {
            SetDatabaseStatus();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private void SetDatabaseStatus()
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("ChapeauDatabaseSQL")
                    ?? throw new InvalidOperationException("Connection string 'ChapeauDatabaseSQL' ontbreekt.");

                using SqlConnection connection = new(connectionString);
                connection.Open();

                ViewBag.DbStatus = "Verbonden met de database.";
            }
            catch (Exception ex)
            {
                ViewBag.DbStatus = $"Niet verbonden met de database: {ex.Message}";
            }
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