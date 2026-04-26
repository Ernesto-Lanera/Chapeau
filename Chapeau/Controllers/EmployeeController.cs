using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IActionResult Index()
        {
            var employees = _employeeService.GetEmployees();
            return View(employees);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (_employeeService.GetEmployees().Any(e => e.Username.Equals(employee.Username, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _employeeService.AddEmployee(employee);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Username", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving to the database.");
                }
            }
            return View(employee);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _employeeService.GetEmployees().FirstOrDefault(e => e.EmployeeID == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(Employee employee)
        {
            if (_employeeService.GetEmployees().Any(e => e.Username.Equals(employee.Username, StringComparison.OrdinalIgnoreCase) && e.EmployeeID != employee.EmployeeID))
            {
                ModelState.AddModelError("Username", "This username is already taken by another employee.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _employeeService.UpdateEmployee(employee);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Username", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving to the database.");
                }
            }
            return View(employee);
        }

        [HttpPost]
        public IActionResult ToggleActive(int id, bool active)
        {
            _employeeService.SetEmployeeActive(id, active);
            return RedirectToAction(nameof(Index));
        }
    }
}