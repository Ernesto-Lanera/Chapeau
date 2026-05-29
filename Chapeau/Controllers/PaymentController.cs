using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanTakeOrders")]
    public class PaymentController : Controller
    {
        private readonly IOrderService _orderService;

        public PaymentController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            List<Order> orders = _orderService.GetAllRunningOrders();
            return View(orders);
        }

        public IActionResult ViewOrder(int tableId)
        {
            try
            {
                Order? order = _orderService.GetActiveOrderByTableId(tableId);
                if (order == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                PaymentOrderViewModel viewModel =
                    _orderService.GetPaymentOrderViewModel(order.OrderId, order.TableNumber);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = ex.Message,
                    RequestId = HttpContext.TraceIdentifier
                };
                return View("Error", errorViewModel);
            }
        }

        public IActionResult Confirmation(int orderId, decimal amount, string method)
        {
            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.Method = method;
            return View();
        }
        public IActionResult CheckoutOrder(int orderId, int tableNumber)
        {
            try
            {
                PaymentOrderViewModel viewModel =
                    _orderService.GetPaymentOrderViewModel(orderId, tableNumber);
                return View("ViewOrder", viewModel);
            }
            catch (Exception ex)
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = ex.Message,
                    RequestId = HttpContext.TraceIdentifier
                };
                return View("Error", errorViewModel);
            }
        }
    }
}