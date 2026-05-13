using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Chapeau.Models;
using Chapeau.ViewModels;
using Chapeau.Services;

namespace Chapeau.Controllers
{
    public class BarController : Controller
    {
        private readonly IOrderService _orderService;

        public BarController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            try
            {
                List<Order> orders = _orderService.GetRunningOrders();

                List<OrderViewModel> viewModels = orders.Select(o => new OrderViewModel
                {
                    OrderID = o.OrderID,
                    TableNumber = o.TableNumber,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    WaitingTime = _orderService.GetWaitingTime(o)
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
    }
}