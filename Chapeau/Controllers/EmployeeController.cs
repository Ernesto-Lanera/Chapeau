using Chapeau.Models;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;
using Chapeau.Repositories.Role;

namespace Chapeau.Controllers
{
    public class EmployeeController(EmployeeService employeeService, IRoleRepository roleRepository) : Controller
    {
        private readonly EmployeeService _employeeService = employeeService;
        private readonly IRoleRepository _roleRepository = roleRepository;

        private const string FlashErrorKey = "FlashError";
        private const string FlashSuccessKey = "FlashSuccess";

        public IActionResult Index(int? editId, bool showCreate = false)
        {
            var employees = _employeeService.GetEmployees();

            ViewBag.Roles = GetRolesSafe();

            ViewBag.EditEmployee = null;
            ViewBag.IsEdit = false;
            ViewBag.ShowCreate = showCreate;

            if (editId.HasValue)
            {
                var employee = employees.FirstOrDefault(e => e.EmployeeID == editId.Value);

                if (employee != null)
                {
                    ViewBag.EditEmployee = employee;
                    ViewBag.IsEdit = true;
                    ViewBag.ShowCreate = false;
                }
                else
                {
                    TempData[FlashErrorKey] = "Medewerker niet gevonden.";
                }
            }

            return View(employees);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return RedirectToAction(nameof(Index), new { showCreate = true });
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = "Gegevens ongeldig. Probeer opnieuw.";
                return RedirectToAction(nameof(Index), new { showCreate = true });
            }

            try
            {
                _employeeService.AddEmployee(employee);
                TempData[FlashSuccessKey] = "Medewerker succesvol toegevoegd.";
            }
            catch (Exception ex)
            {
                TempData[FlashErrorKey] = $"Fout bij toevoegen medewerker: {ex.Message}";
                return RedirectToAction(nameof(Index), new { showCreate = true });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Update(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = "Gegevens ongeldig. Probeer opnieuw.";
                return RedirectToAction(nameof(Index), new { editId = employee.EmployeeID });
            }

            try
            {
                _employeeService.UpdateEmployee(employee);
                TempData[FlashSuccessKey] = "Medewerker succesvol bijgewerkt.";
            }
            catch (Exception ex)
            {
                TempData[FlashErrorKey] = $"Fout bij bijwerken medewerker: {ex.Message}";
                return RedirectToAction(nameof(Index), new { editId = employee.EmployeeID });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult SetActive(int id, bool active)
        {
            try
            {
                _employeeService.SetEmployeeActive(id, active);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        private List<Role> GetRolesSafe()
        {
            try
            {
                return _roleRepository.GetRoles();
            }
            catch
            {
                return new List<Role>();
            }
        }
    }
}