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
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee employee, string? Password)
        {
            if (!string.IsNullOrWhiteSpace(Password))
            {
                employee.PasswordHash = Password;
            }

            ValidateEmployeeForCreate(employee);

            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = GetModelStateErrors();
                return RedirectToAction(nameof(Index), new { showCreate = true });
            }

            try
            {
                employee.IsActive = true;

                _employeeService.AddEmployee(employee);

                TempData[FlashSuccessKey] = "Medewerker succesvol toegevoegd.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[FlashErrorKey] = $"Fout bij toevoegen medewerker: {ex.Message}";
                return RedirectToAction(nameof(Index), new { showCreate = true });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Employee employee, string? Password)
        {
            return UpdateEmployee(employee, Password);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Employee employee, string? Password)
        {
            return UpdateEmployee(employee, Password);
        }

        private IActionResult UpdateEmployee(Employee employee, string? Password)
        {
            var existingEmployee = _employeeService.GetEmployees()
                .FirstOrDefault(e => e.EmployeeID == employee.EmployeeID);

            if (existingEmployee == null)
            {
                TempData[FlashErrorKey] = "Medewerker niet gevonden.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove(nameof(Employee.PasswordHash));
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Password");

            ValidateEmployeeForUpdate(employee);

            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = GetModelStateErrors();
                return RedirectToAction(nameof(Index), new { editId = employee.EmployeeID });
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    employee.PasswordHash = Password;
                }
                else if (string.IsNullOrWhiteSpace(employee.PasswordHash))
                {
                    employee.PasswordHash = existingEmployee.PasswordHash;
                }

                employee.IsActive = existingEmployee.IsActive;

                _employeeService.UpdateEmployee(employee);

                TempData[FlashSuccessKey] = "Medewerker succesvol bijgewerkt.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[FlashErrorKey] = $"Fout bij bijwerken medewerker: {ex.Message}";
                return RedirectToAction(nameof(Index), new { editId = employee.EmployeeID });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetActive(int id, bool active)
        {
            try
            {
                _employeeService.SetEmployeeActive(id, active);

                TempData[FlashSuccessKey] = active
                    ? "Medewerker succesvol geactiveerd."
                    : "Medewerker succesvol gedeactiveerd.";
            }
            catch (Exception ex)
            {
                TempData[FlashErrorKey] = $"Fout bij aanpassen status: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateEmployeeForCreate(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                ModelState.AddModelError(nameof(Employee.Name), "Naam is verplicht.");
            }

            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                ModelState.AddModelError(nameof(Employee.PasswordHash), "Wachtwoord/pincode is verplicht.");
            }

            if (employee.RoleID <= 0)
            {
                ModelState.AddModelError(nameof(Employee.RoleID), "Kies een geldige rol.");
            }
        }

        private void ValidateEmployeeForUpdate(Employee employee)
        {
            if (employee.EmployeeID <= 0)
            {
                ModelState.AddModelError(nameof(Employee.EmployeeID), "Ongeldige medewerker.");
            }

            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                ModelState.AddModelError(nameof(Employee.Name), "Naam is verplicht.");
            }

            if (employee.RoleID <= 0)
            {
                ModelState.AddModelError(nameof(Employee.RoleID), "Kies een geldige rol.");
            }
        }

        private string GetModelStateErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToList();

            if (!errors.Any())
            {
                return "Gegevens ongeldig. Probeer opnieuw.";
            }

            return string.Join("\n", errors);
        }

        private List<Role> GetRolesSafe()
        {
            try
            {
                return _roleRepository.GetRoles()
                    .GroupBy(role => role.RoleID)
                    .Select(group => group.First())
                    .OrderBy(role => role.RoleName)
                    .ToList();
            }
            catch
            {
                return new List<Role>();
            }
        }
    }
}