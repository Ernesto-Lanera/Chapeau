using Chapeau.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace Chapeau.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                // Haal de connection string veilig uit the configuratie (User Secrets)
                string connectionString = _configuration.GetConnectionString("ChapeauDatabaseSQL") 
                    ?? throw new Exception("Database connection string is missing.");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    ViewBag.DbStatus = "Verbonden met de Azure SQL Database!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.DbStatus = $"Fout bij verbinden met de database: {ex.Message}";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
