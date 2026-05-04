using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;

namespace Someren.Controllers
{
    public class DrinkController : Controller
    {
        private readonly IDrinkRepository _DrinksRepository;

        public DrinkController(IDrinkRepository DrinksRepository)
        {
            _DrinksRepository = DrinksRepository;
        }

        public IActionResult Index()
        {
            List<Drink> Drinks = _DrinksRepository.GetAll();
            return View(Drinks);
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
            Drink? Drink = _DrinksRepository.GetById((int)id);
            return View(Drink);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Drink? Drink = _DrinksRepository.GetById((int)id);
            return View(Drink);
        }

        [HttpPost]
        public ActionResult Create(Drink Drink)
        {
            try
            {
                _DrinksRepository.Add(Drink);
                TempData["AlertMessage"] = "Drink added successfully!";
                TempData["AlertType"] = "alert-success";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Error: " + ex.Message;
                TempData["AlertType"] = "alert-danger";
                return View(Drink);
            }
        }

        [HttpPost]
        public ActionResult Edit(Drink Drink)
        {
            try
            {
                _DrinksRepository.Update(Drink);
                TempData["AlertMessage"] = $"Drink with Drinknumber {Drink.DrinkId} Edited successfully!";
                TempData["AlertType"] = "alert-success";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Error: " + ex.Message;
                TempData["AlertType"] = "alert-danger";
                return View(Drink);
            }
        }



        [HttpPost]
        public ActionResult Delete(Drink Drink)
        {
            try
            {
                _DrinksRepository.Delete(Drink);
                TempData["AlertMessage"] = $"Drink with Drinknumber {Drink.DrinkId} Deleted successfully!";
                TempData["AlertType"] = "alert-success";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Error: " + ex.Message;
                TempData["AlertType"] = "alert-danger";
                return View(Drink);
            }
        }


    }
}

