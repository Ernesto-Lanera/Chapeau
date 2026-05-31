using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

                OrderItems = o.OrderItems
               .OrderBy(i => i.Value.Course)
               .Select(i => new OrderItemViewModel
                {
                    Name = i.Value.MenuItemName,
                    Amount = i.Value.Amount,
                    Comment = i.Value.Comment
                }).ToList(),

                            CourseGroups = o.OrderItems
                .OrderBy(i => i.Value.Course)
                .GroupBy(i => i.Value.Course)
                .Select(g => new CourseGroupViewModel
                {
                    Course = g.Key ?? CourseType.Starter,
                    Items = g.Select(i => new OrderItemViewModel
                    {
                        Name = i.Value.MenuItemName,
                        Amount = i.Value.Amount,
                        Comment = i.Value.Comment
                    }).ToList()
                }).ToList()

            }).ToList();

            return View(viewModels);
        }

       
    }
}