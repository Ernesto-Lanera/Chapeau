using Chapeau.Emums;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Table controller for displaying table status and marking orders as served.
    /// </summary>
    public class TableController : Controller
    {
        private readonly IOrderService _orderService;

        public TableController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Display all table statuses with active order information.
        /// </summary>
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

        /// <summary>
        /// Mark an order as served (completed order, ready for payment).
        /// </summary>
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
