using Chapeau.Models;
using Chapeau.Services;
using Chapeau.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    public class EmployeeController(EmployeeService employeeService, RoleRepository roleRepository) : Controller
    {
        private readonly EmployeeService _employeeService = employeeService;
        private readonly RoleRepository _roleRepository = roleRepository;

        public IActionResult Index()
        {
            var employees = _employeeService.GetEmployees();
            return View(employees);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _roleRepository.GetRoles();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _employeeService.AddEmployee(employee);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving to the database.");
                }
            }
            ViewBag.Roles = _roleRepository.GetRoles();
            return View(employee);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _employeeService.GetEmployees()
                .FirstOrDefault(e => e.EmployeeID == id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Roles = _roleRepository.GetRoles();
            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // If password is empty, keep the existing password
                    if (string.IsNullOrWhiteSpace(employee.PasswordHash))
                    {
                        var existingEmployee = _employeeService.GetEmployees()
                            .FirstOrDefault(e => e.EmployeeID == employee.EmployeeID);

                        if (existingEmployee != null)
                        {
                            employee.PasswordHash = existingEmployee.PasswordHash;
                        }
                    }

                    _employeeService.UpdateEmployee(employee);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving to the database.");
                }
            }
            ViewBag.Roles = _roleRepository.GetRoles();
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