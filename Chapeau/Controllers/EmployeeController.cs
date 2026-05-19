using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Services.Overview;
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

        // Toont alle medewerkers met edit optie
        public IActionResult Index(int? editId, bool showCreate = false)
        {
            var employees = _employeeService.GetEmployees();
            ViewBag.Roles = GetRolesSafe();
            ViewBag.ShowCreate = showCreate;

            if (editId.HasValue)
            {
                var employee = employees.FirstOrDefault(e => e.EmployeeID == editId);
                if (employee != null)
                {
                    ViewBag.EditEmployee = employee;
                    ViewBag.IsEdit = true;
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
            return RedirectToAction(nameof(Index), new
            {
                showCreate = true
            });
        }

        // Maakt nieuwe medewerker aan
        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            employee.IsActive = true;

            ValidateEmployee(employee, isNew: true);

            if (!ModelState.IsValid)
            {
                TempData[NewEmployeeDraftKey] = JsonSerializer.Serialize(new
                {
                    employee.Name,
                    employee.RoleID
                });

                TempData[FlashErrorKey] = GetModelStateErrors();

                return RedirectToAction(nameof(Index), new
                {
                    showCreate = true
                });
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

                return RedirectToAction(nameof(Index), new
                {
                    showCreate = true
                });
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is iets misgegaan bij het opslaan. Probeer het opnieuw.";

                return RedirectToAction(nameof(Index), new
                {
                    showCreate = true
                });
            }
        }

        [HttpPost]
        public IActionResult QuickCreate(Employee employee)
        {
            return Create(employee);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return RedirectToAction(nameof(Index), new
            {
                editId = id
            });
        }

        // Werkt medewerker bij
        [HttpPost]
        public IActionResult Edit(Employee employee)
        {
            ModelState.Remove(nameof(Employee.PasswordHash));

            var existingEmployee = _employeeService.GetEmployees()
                .FirstOrDefault(e => e.EmployeeID == employee.EmployeeID);

            if (existingEmployee == null)
            {
                TempData[FlashErrorKey] = "Medewerker bestaat niet meer.";
                return RedirectToAction(nameof(Index));
            }

            employee.IsActive = existingEmployee.IsActive;

            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                employee.PasswordHash = existingEmployee.PasswordHash;
            }

            ValidateEmployee(employee, isNew: false);

            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = GetModelStateErrors();

                return RedirectToAction(nameof(Index), new
                {
                    editId = employee.EmployeeID
                });
            }

            try
            {
                _employeeService.UpdateEmployee(employee);

                TempData[FlashSuccessKey] = "Medewerker bijgewerkt.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData[FlashErrorKey] = ex.Message;

                return RedirectToAction(nameof(Index), new
                {
                    editId = employee.EmployeeID
                });
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is een onverwachte fout opgetreden bij het opslaan.";

                return RedirectToAction(nameof(Index), new
                {
                    editId = employee.EmployeeID
                });
            }
        }

        // Zet medewerker status aan/uit
        [HttpPost]
        public IActionResult ToggleActive(int id, bool active)
        {
            try
            {
                _employeeService.SetEmployeeActive(id, active);

                TempData[FlashSuccessKey] = active
                    ? "Medewerker geactiveerd."
                    : "Medewerker gedeactiveerd.";
            }
            catch
            {
                TempData[FlashErrorKey] = "Kon status niet aanpassen. Probeer opnieuw.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Valideert medwerkerinvoer
        private void ValidateEmployee(Employee employee, bool isNew)
        {
            var roles = GetRolesSafe();

            if (string.IsNullOrWhiteSpace(employee.Name))
                ModelState.AddModelError(nameof(Employee.Name), "Naam is verplicht.");

            if (employee.RoleID <= 0 || !roles.Any(r => r.RoleID == employee.RoleID))
                ModelState.AddModelError(nameof(Employee.RoleID), "Kies een geldige rol.");

            if (isNew && string.IsNullOrWhiteSpace(employee.PasswordHash))
                ModelState.AddModelError(nameof(Employee.PasswordHash), "Wachtwoord/Pincode is verplicht.");
        }

        // Haalt rollen veilig op
        private List<EmployeeRole> GetRolesSafe()
        {
            try
            {
                return _roleRepository.GetRoles();
            }
            catch
            {
                TempData[FlashErrorKey] = "Kon rollen niet ophalen. Controleer database/connectionstring.";
                return new List<EmployeeRole>();
            }
        }

        // Verzamelt validatie fouten
        private string GetModelStateErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .Distinct()
                .ToList();

            return string.Join("\n", errors);
        }
    }
}