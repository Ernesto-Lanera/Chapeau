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
            catch (ArgumentException ex)
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = ex.Message,
                    RequestId = HttpContext.TraceIdentifier
                };
                return View("Error", errorViewModel);
            }
            catch (InvalidOperationException ex)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Complete(int orderId, decimal amount, decimal tipAmount, string method, string? feedback)
        {
            try
            {
                _orderService.CompletePayment(orderId, tipAmount, feedback);
                ViewBag.OrderId = orderId;
                ViewBag.Amount = amount;
                ViewBag.Method = method;
                ViewBag.TipAmount = tipAmount;
                ViewBag.Feedback = feedback;
                return View("Confirmation");
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