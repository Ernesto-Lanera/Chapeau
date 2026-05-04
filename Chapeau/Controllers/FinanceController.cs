using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class FinanceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}