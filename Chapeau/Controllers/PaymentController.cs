using System.Linq;
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
            List<Order> orders = _orderService.GetServedOrdersForPayment();
            return View(orders);
        }

        public IActionResult ViewOrder(int tableId)
        {
            try
            {
                var order = _orderService.GetActiveOrderByTableId(tableId);
                if (order == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return View(BuildPaymentViewModel(order.OrderId, order.TableNumber));
               
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
                return View("ViewOrder", BuildPaymentViewModel(orderId, tableNumber));
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
                if (request == null || request.OrderId <= 0)
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

        private PaymentOrderViewModel BuildPaymentViewModel(int orderId, int tableNumber)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Ongeldig order ID.", nameof(orderId));
            }

            if (tableNumber <= 0)
            {
                throw new ArgumentException("Ongeldig tafel nummer.", nameof(tableNumber));
            }

            List<OrderItem> items = _orderService.GetOrderItemsByOrderId(orderId);
            if (items == null || items.Count == 0)
            {
                throw new InvalidOperationException($"No items found for order {orderId}.");
            }

            var groupedItems = items
                .GroupBy(i => i.MenuItemId)
                .Select(g =>
                {
                    var firstItem = g.First();
                    return new OrderItem
                    {
                        OrderItemId = firstItem.OrderItemId,
                        MenuItemId = g.Key,
                        MenuItem = firstItem.MenuItem,
                        Name = firstItem.Name ?? "Unknown Item",
                        Price = firstItem.Price,
                        VATRate = firstItem.VATRate,
                        Amount = g.Sum(x => x.Amount),
                        Comment = firstItem.Comment,
                        OrderId = orderId
                    };
                })
                .ToList();

            foreach (var item in groupedItems)
            {
                ValidatePaymentItem(item);
            }

            var order = new Order { Items = groupedItems };

            return new PaymentOrderViewModel
            {
                OrderID = orderId,
                TableNumber = tableNumber,
                Items = groupedItems.AsReadOnly(),
                LowVAT = Math.Round(order.LowVATTotal, 2),
                HighVAT = Math.Round(order.HighVATTotal, 2),
                Total = Math.Round(order.GrandTotal, 2)
            };
        }

        private static void ValidatePaymentItem(OrderItem item)
        {
            if (string.IsNullOrEmpty(item.Name))
                throw new InvalidOperationException($"Item {item.MenuItemId} has no name.");

            if (item.Price < 0)
                throw new InvalidOperationException($"Invalid price for item {item.Name}: prices cannot be negative.");

            if (item.VATRate < 0 || item.VATRate > 1)
                throw new InvalidOperationException($"Invalid VAT rate for item {item.Name}: VAT must be between 0 and 1.");

            if (item.Amount <= 0)
                throw new InvalidOperationException($"Invalid quantity for item {item.Name}: quantity must be greater than zero.");
        }

        private IActionResult Error(Exception ex)
        {
            return View("Error", new ErrorViewModel
               
                return View();
            }
            catch (InvalidOperationException ex)
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