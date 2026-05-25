using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Chapeau.Models;
using Chapeau.ViewModels;
using Chapeau.Services;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Bar controller for displaying running orders (drinks/bar view).
    /// </summary>
    public class BarController : Controller
    {
        private readonly IOrderService _orderService;

        public BarController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Display all running orders with waiting time for bar staff.
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                List<Order> orders = _orderService.GetRunningOrders();

                List<OrderViewModel> viewModels = orders.Select(o => new OrderViewModel
                {
                    OrderID = o.OrderId,
                    TableNumber = o.TableNumber,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    WaitingTime = _orderService.GetWaitingTime(o)
                }).ToList();

                return View(viewModels);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
    }
}