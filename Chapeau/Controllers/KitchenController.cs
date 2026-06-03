using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanPrepareFood")]
    public class KitchenController : Controller
    {
        private readonly IOrderService _orderService;

        public KitchenController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize(Roles = "Keuken")]
        public IActionResult Index()
        {
            List<Order> orders = _orderService.GetRunningOrders(OrderType.Food);

            List<OrderViewModel> viewModels = orders.Select(o => new OrderViewModel
            {
                OrderID = o.OrderId,
                TableNumber = o.TableNumber,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                WaitingTime = _orderService.GetWaitingTime(o),
                ControllerName = "Kitchen",
                OrderType = OrderType.Food,

                OrderItems = o.OrderItems
                    .OrderBy(i => i.Course)
                    .Select(i => new OrderItemViewModel
                    {
                        OrderItemId = i.OrderItemId,
                        Name = i.MenuItemName,
                        Amount = i.AmountOrdered,
                        Comment = i.Comment,
                        Status = i.OrderItemStatus
                    }).ToList(),

                CourseGroups = o.OrderItems
                    .OrderBy(i => i.Course)
                    .GroupBy(i => i.Course)
                    .Select(g => new CourseGroupViewModel
                    {
                        Course = g.Key ?? CourseType.Starter,
                        Items = g.Select(i => new OrderItemViewModel
                        {
                            OrderItemId = i.OrderItemId,
                            Name = i.MenuItemName,
                            Amount = i.AmountOrdered,
                            Comment = i.Comment,
                            Status = i.OrderItemStatus
                        }).ToList()
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
        public IActionResult PrepareAllItems(int orderId, int? course)
        {
            if (course.HasValue)
                _orderService.UpdateCourseItemStatuses(orderId, (CourseType)course.Value, OrderStatus.BeingPrepared);
            else
                _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Food, OrderStatus.BeingPrepared);
            _orderService.UpdateOrderStatus(orderId, OrderStatus.BeingPrepared);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ReadyAllItems(int orderId, int? course)
        {
            if (course.HasValue)
                _orderService.UpdateCourseItemStatuses(orderId, (CourseType)course.Value, OrderStatus.ReadyToBeServed);
            else
                _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Food, OrderStatus.ReadyToBeServed);
            _orderService.UpdateOrderIfServed(orderId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MarkOrderReady(int orderId)
        {
            _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Food, OrderStatus.ReadyToBeServed);
            _orderService.UpdateOrderIfServed(orderId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Finished()
        {
            List<Order> orders = _orderService.GetFinishedOrdersToday(OrderType.Food);

            List<OrderViewModel> viewModels = orders.Select(o => new OrderViewModel
            {
                OrderID = o.OrderId,
                TableNumber = o.TableNumber,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                OrderItems = o.OrderItems
                    .OrderBy(i => i.Course)
                    .Select(i => new OrderItemViewModel
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