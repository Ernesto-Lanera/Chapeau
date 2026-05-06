using Chapeau.Models;
using Chapeau.Services;
using Chapeau.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Chapeau.Controllers
{
    public class EmployeeController(EmployeeService employeeService, RoleRepository roleRepository) : Controller
    {
        private readonly EmployeeService _employeeService = employeeService;
        private readonly RoleRepository _roleRepository = roleRepository;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";
        private const string NewEmployeeDraftKey = "NewEmployeeDraft";

        public IActionResult Index()
        {
            var employees = _employeeService.GetEmployees();
            try
            {
                ViewBag.Roles = _roleRepository.GetRoles();
            }
            catch
            {
                ViewBag.Roles = new List<Role>();
                TempData[FlashErrorKey] = "Kon rollen niet ophalen. Controleer database/connectionstring.";
            }
            return View(employees);
        }

        [HttpPost]
        public IActionResult QuickCreate(Employee employee)
        {
            var roles = _roleRepository.GetRoles();
            TempData[NewEmployeeDraftKey] = JsonSerializer.Serialize(new
            {
                employee.Name,
                employee.RoleID
            });

            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                ModelState.AddModelError(nameof(Employee.Name), "Naam is verplicht.");
            }
            if (employee.RoleID <= 0)
            {
                ModelState.AddModelError(nameof(Employee.RoleID), "Kies een geldige rol.");
            }
            else if (!roles.Any(r => r.RoleID == employee.RoleID))
            {
                ModelState.AddModelError(nameof(Employee.RoleID), "Kies een rol uit de lijst.");
            }
            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                ModelState.AddModelError(nameof(Employee.PasswordHash), "Wachtwoord/Pincode is verplicht.");
            }

            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = string.Join("\n",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _employeeService.AddEmployee(employee);
                TempData[FlashSuccessKey] = "Medewerker toegevoegd.";
                TempData.Remove(NewEmployeeDraftKey);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData[FlashErrorKey] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is iets misgegaan bij het opslaan. Probeer het opnieuw.";
                return RedirectToAction(nameof(Index));
            }
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
                    ModelState.AddModelError("", "Er is een onverwachte fout opgetreden bij het opslaan.");
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
                    ModelState.AddModelError("", "Er is een onverwachte fout opgetreden bij het opslaan.");
                }
            }
            ViewBag.Roles = _roleRepository.GetRoles();
            return View(employee);
        }

        [HttpPost]
        public IActionResult ToggleActive(int id, bool active)
        {
            try
            {
                _employeeService.SetEmployeeActive(id, active);
                TempData[FlashSuccessKey] = active ? "Medewerker geactiveerd." : "Medewerker gedeactiveerd.";
            }
            catch
            {
                TempData[FlashErrorKey] = "Kon status niet aanpassen. Probeer opnieuw.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}