using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Chapeau.Controllers
{
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