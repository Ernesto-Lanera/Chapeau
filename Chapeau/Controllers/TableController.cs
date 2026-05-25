using Chapeau.Emums;
using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanTakeOrders")]
    public class TableController : Controller
    {
        private readonly IOrderService _orderService;

        public TableController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public IActionResult Index()
        {
            try
            {
                var tables = _orderService.GetAllTableStatuses();
                return View(tables);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsServed(int orderId)
        {
            try
            {
                _orderService.MarkOrderAsServed(orderId);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
    }
}
