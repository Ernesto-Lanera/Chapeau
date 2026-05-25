using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Payment controller for handling order payment and viewing payment details.
    /// </summary>
    public class PaymentController : Controller
    {
        private readonly IOrderService _orderService;

        public PaymentController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Display all running orders for payment selection.
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                List<Order> orders = _orderService.GetRunningOrders();
                return View(orders);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        /// <summary>
        /// Display detailed payment information for a specific order.
        /// </summary>
        public IActionResult ViewOrder(int tableId)
        {
            try
            {
                Order? order = _orderService.GetActiveOrderByTableId(tableId);
                if (order == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                PaymentOrderViewModel viewModel = _orderService.GetPaymentOrderViewModel(order.OrderId, order.TableNumber);
                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
    }
}
