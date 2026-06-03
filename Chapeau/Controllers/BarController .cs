using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanPrepareDrinks")]
    public class BarController : Controller
    {
        private readonly IOrderService _orderService;

        public BarController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize(Roles = "Barman")]
        public IActionResult Index()
        {
            List<Order> orders = _orderService.GetRunningOrders(OrderType.Drink);

            List<OrderViewModel> viewModels = orders.Select(o => new OrderViewModel
            {
                OrderID = o.OrderId,
                TableNumber = o.TableNumber,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                WaitingTime = _orderService.GetWaitingTime(o),
                ControllerName = "Bar",
                OrderType = OrderType.Drink,

                OrderItems = o.OrderItems.Select(i => new OrderItemViewModel
                {
                    OrderItemId = i.OrderItemId,
                    Name = i.MenuItemName,
                    Amount = i.AmountOrdered,
                    Comment = i.Comment,
                    Status = i.OrderItemStatus
                }).ToList()
            }).ToList();

            return View(viewModels);
        }

        [HttpPost]
        public IActionResult UpdateOrderItemStatus(int orderItemId, int orderId, int status)
        {
            _orderService.UpdateOrderItemStatus(orderItemId, (OrderStatus)status);
            _orderService.UpdateOrderIfServed(orderId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult PrepareAllItems(int orderId)
        {
            _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Drink, OrderStatus.BeingPrepared);
            _orderService.UpdateOrderStatus(orderId, OrderStatus.BeingPrepared);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ReadyAllItems(int orderId)
        {
            _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Drink, OrderStatus.ReadyToBeServed);
            _orderService.UpdateOrderIfServed(orderId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MarkOrderReady(int orderId)
        {
            _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Drink, OrderStatus.ReadyToBeServed);
            _orderService.UpdateOrderIfServed(orderId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Finished()
        {
            List<Order> orders = _orderService.GetFinishedOrdersToday(OrderType.Drink);

            List<OrderViewModel> viewModels = orders.Select(o => new OrderViewModel
            {
                OrderID = o.OrderId,
                TableNumber = o.TableNumber,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                OrderItems = o.OrderItems.Select(i => new OrderItemViewModel
                {
                    OrderItemId = i.OrderItemId,
                    Name = i.MenuItemName,
                    Amount = i.AmountOrdered,
                    Comment = i.Comment,
                    Status = i.OrderItemStatus
                }).ToList()
            }).ToList();

            return View(viewModels);
        }
    }
}