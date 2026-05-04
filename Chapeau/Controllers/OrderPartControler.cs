using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;

namespace Someren.Controllers
{
    public class OrderPartController : Controller
    {
        private readonly IOrderPartRepository _orderPartRepository;

        public OrderPartController(IOrderPartRepository orderPartRepository)
        {
            _orderPartRepository = orderPartRepository;
        }


        [HttpGet]
        public IActionResult Create(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        
        [HttpPost]
        public IActionResult Create(OrderPart orderPart)
        {
            _orderPartRepository.Add(orderPart);

            TempData["AlertMessage"] = "Drink successfully added to the order!";
            TempData["AlertType"] = "alert-success";

            return RedirectToAction("Index", "Order");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {

            OrderPart orderPart = _orderPartRepository.GetById(id);

            if (orderPart == null)
            {
                return NotFound();
            }

            return View(orderPart);
        }

        [HttpPost]
        public IActionResult Edit(OrderPart orderPart)
        {
            // Save the changes to the database
            _orderPartRepository.Update(orderPart);

            TempData["AlertMessage"] = "Drink amount successfully updated!";
            TempData["AlertType"] = "alert-success";

            return RedirectToAction("Index", "Order");
        }



        [HttpGet]
        public IActionResult Delete(int id)
        {
            OrderPart orderPart = _orderPartRepository.GetById(id);

            if (orderPart == null)
            {
                return NotFound();
            }

            return View(orderPart);
        }

        [HttpPost]
        public IActionResult Delete(OrderPart orderPart)
        {
            _orderPartRepository.Delete(orderPart);

            TempData["AlertMessage"] = "Drink successfully removed from the order!";
            TempData["AlertType"] = "alert-warning"; 

            return RedirectToAction("Index", "Order");
        }
    }
}
