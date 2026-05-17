using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class TableController : Controller
    {
        private readonly IOrderService _orderService;

        public TableController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            var tables = _orderService.GetAllTableStatuses();
            return View(tables);
        }
    }
}
