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
                var order = _orderService.GetActiveOrderByTableId(tableId);
                return order == null
                    ? RedirectToAction(nameof(Index))
                    : View(_orderService.GetPaymentOrderViewModel(order.OrderId, order.TableNumber));
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        public IActionResult CheckoutOrder(int orderId, int tableNumber)
        {
            try
            {
                return View("ViewOrder", _orderService.GetPaymentOrderViewModel(orderId, tableNumber));
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        [HttpPost]
        public IActionResult SavePayment([FromBody] PaymentSaveRequest request)
        {
            try
            {
                if (request?.OrderId <= 0)
                {
                    return Json(new { success = false, message = "Invalid order ID" });
                }

                _orderService.SavePayment(request.OrderId, request.TableNumber, request.TipAmount, request.Feedback);
                return Json(new { success = true, message = "Payment saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private IActionResult Error(Exception ex)
        {
            return View("Error", new ErrorViewModel
            {
                ErrorMessage = ex.Message,
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }

    //nodig voor json in payment.js
    public class PaymentSaveRequest
    {
        public int OrderId { get; set; }
        public int TableNumber { get; set; }
        public decimal TipAmount { get; set; }
        public string? Feedback { get; set; }
    }
}