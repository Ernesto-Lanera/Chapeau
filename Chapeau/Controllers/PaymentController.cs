using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IOrderService _orderService;

        public PaymentController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            
            List<Order> orders = _orderService.GetRunningOrders();
            return View(orders);
        }

        public IActionResult ViewOrder(int tableId)
        {
            Order order = _orderService.GetActiveOrderByTableId(tableId);
            if (order == null)
                return View("Index");
            
            PaymentOrderViewModel viewModel = _orderService.GetPaymentOrderViewModel(order.OrderId, order.TableNumber);
            return View(viewModel);
        }
    }
}
