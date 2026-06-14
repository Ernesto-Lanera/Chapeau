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

        public IActionResult Index()
        {
            try
            {
                List<Order> orders = _orderService.GetRunningOrders(OrderType.Food);
                List<OrderViewModel> viewModels = orders.Select(MapToViewModel).ToList();
                return View(viewModels);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Something went wrong loading the orders.");
            }
        }

        [HttpPost]
        public IActionResult UpdateOrderItemStatus(int orderItemId, int orderId, int status)
        {
            try
            {
                _orderService.UpdateOrderItemStatus(orderItemId, (OrderStatus)status);
                _orderService.UpdateOrderIfServed(orderId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Something went wrong updating the order status.");
            }
        }

        [HttpPost]
        public IActionResult PrepareAllItems(int orderId, int? course)
        {
            try
            {
                if (course.HasValue)
                    _orderService.UpdateCourseItemStatuses(orderId, (CourseType)course.Value, OrderStatus.BeingPrepared);
                else
                    _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Food, OrderStatus.BeingPrepared);

                _orderService.UpdateOrderStatus(orderId, OrderStatus.BeingPrepared);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Something went wrong preparing the items.");
            }
        }

        [HttpPost]
        public IActionResult ReadyAllItems(int orderId, int? course)
        {
            try
            {
                if (course.HasValue)
                    _orderService.UpdateCourseItemStatuses(orderId, (CourseType)course.Value, OrderStatus.ReadyToBeServed);
                else
                    _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Food, OrderStatus.ReadyToBeServed);

                _orderService.UpdateOrderIfServed(orderId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Something went wrong marking the items as ready.");
            }
        }

        [HttpPost]
        public IActionResult MarkOrderReady(int orderId)
        {
            try
            {
                _orderService.UpdateAllOrderItemStatuses(orderId, OrderType.Food, OrderStatus.ReadyToBeServed);
                _orderService.UpdateOrderIfServed(orderId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Something went wrong marking the order as ready.");
            }
        }

        [HttpGet]
        public IActionResult Finished()
        {
            try
            {
                List<Order> orders = _orderService.GetFinishedOrdersToday(OrderType.Food);
                List<OrderViewModel> viewModels = orders.Select(MapToFinishedViewModel).ToList();
                return View(viewModels);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Something went wrong loading the finished orders.");
            }
        }

        private OrderViewModel MapToViewModel(Order o)
        {
            return new OrderViewModel
            {
                OrderID = o.OrderId,
                TableNumber = o.TableNumber,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                WaitingTime = _orderService.GetWaitingTime(o),
                ControllerName = "Kitchen",
                OrderType = OrderType.Food,
                OrderItems = MapItems(o.OrderItems ?? new List<OrderItem>()),
                CourseGroups = MapCourseGroups(o.OrderItems ?? new List<OrderItem>())
            };
        }

        private static OrderViewModel MapToFinishedViewModel(Order o)
        {
            return new OrderViewModel
            {
                OrderID = o.OrderId,
                TableNumber = o.TableNumber,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                OrderItems = MapItems(o.OrderItems ?? new List<OrderItem>())
            };
        }

        private static List<OrderItemViewModel> MapItems(List<OrderItem> items)
        {
            return items
                .OrderBy(i => i.Course)
                .Select(i => new OrderItemViewModel
                {
                    OrderItemId = i.OrderItemId,
                    Name = i.MenuItemName,
                    Amount = i.AmountOrdered,
                    Comment = i.Comment,
                    Status = i.OrderItemStatus
                }).ToList();
        }

        private static List<CourseGroupViewModel> MapCourseGroups(List<OrderItem> items)
        {
            return items
                .GroupBy(i => i.Course ?? CourseType.Starter)
                .Select(g => new CourseGroupViewModel
                {
                    Course = g.Key,
                    Items = g.Select(i => new OrderItemViewModel
                    {
                        OrderItemId = i.OrderItemId,
                        Name = i.MenuItemName,
                        Amount = i.AmountOrdered,
                        Comment = i.Comment,
                        Status = i.OrderItemStatus
                    }).ToList()
                })
                .OrderBy(g => g.Course)
                .ToList();
        }

        private IActionResult HandleError(Exception ex, string message)
        {
            TempData["Error"] = message;
            return RedirectToAction("Index");
        }
    }
}