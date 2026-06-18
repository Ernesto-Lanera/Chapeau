using Chapeau.Models;
using Chapeau.Services;
using Chapeau.Services.Payment;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanTakeOrders")]
    public class PaymentController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public PaymentController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
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
                var order = _orderService.GetServedOrderByTableId(tableId);
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

                _paymentService.SavePayment(request.OrderId, request.TipAmount, request.Feedback);
                return Json(new { success = true, message = "Betaling opgeslagen" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Confirmation(int orderId, decimal amount, string method)
        {
            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.Method = method;
            return View();
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
                OrderIDs = new List<int> { orderId },
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
            {
                ErrorMessage = ex.Message,
                RequestId = HttpContext.TraceIdentifier
            });
        }
        public IActionResult CheckoutTable(int tableId) //via tableid want dat is sneller.
        {
            var orders = _orderService.GetServedOrdersByTableId(tableId);

            if (!orders.Any())
                return RedirectToAction("Index");

            var allItems = orders.SelectMany(o => o.Items ?? new List<OrderItem>()).ToList();

            var viewModel = new PaymentOrderViewModel
            {
                TableNumber = orders.First().TableNumber, //table number is zelfde voor alle orders van die tafel.
                OrderID = orders.First().OrderId,
                OrderIDs = orders.Select(o => o.OrderId).ToList(),
                Items = allItems,
                LowVAT = allItems.Where(i => i.VATRate == 0.06m).Sum(i => i.Price * i.Amount * i.VATRate),
                HighVAT = allItems.Where(i => i.VATRate == 0.21m).Sum(i => i.Price * i.Amount * i.VATRate),
                Total = allItems.Sum(i => i.Price * i.Amount * (1 + i.VATRate))
            };

            return View("ViewOrder", viewModel);
        }
    }    
}