using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;

namespace Someren.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _OrdersRepository;

        public OrderController(IOrderRepository OrdersRepository)
        {
            _OrdersRepository = OrdersRepository;
        }

        public IActionResult Index()
        {
            List<Order> Orders = _OrdersRepository.GetAll();
            return View(Orders);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();

        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Order? Order = _OrdersRepository.GetById((int)id);
            return View(Order);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {   
            if (id == null)
            {
                return NotFound();
            }
            Order? Order = _OrdersRepository.GetById((int)id);
            return View(Order);
        }

        [HttpPost]
        public ActionResult Create(Order Order)
        {
            try
            {
                _OrdersRepository.Add(Order);
                TempData["AlertMessage"] = "Order added successfully!";
                TempData["AlertType"] = "alert-success";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Error: " + ex.Message;
                TempData["AlertType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(Order Order)
        {
            try
            {
                _OrdersRepository.Update(Order);
                TempData["AlertMessage"] = $"Order with Ordernumber {Order.OrderId} Edited successfully!";
                TempData["AlertType"] = "alert-success";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Error: " + ex.Message;
                TempData["AlertType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }



        [HttpPost]
        public ActionResult Delete(Order Order)
        {
            try
            {
                _OrdersRepository.Delete(Order);
                TempData["AlertMessage"] = $"Order with Ordernumber {Order.OrderId} Deleted successfully!";
                TempData["AlertType"] = "alert-success";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Error: " + ex.Message;
                TempData["AlertType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


    }
}
