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
            return RedirectToAction(nameof(Index), new
            {
                showCreate = true
            });
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            employee.IsActive = true;

            ValidateEmployeeForCreate(employee);

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

                // Succes: form sluiten
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData[FlashErrorKey] = ex.Message;

                // Fout: form open houden
                return RedirectToAction(nameof(Index), new
                {
                    showCreate = true
                });
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is iets misgegaan bij het opslaan. Probeer het opnieuw.";

                // Fout: form open houden
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

        [HttpPost]
        public IActionResult Edit(Employee employee)
        {
            // Belangrijk:
            // PasswordHash is bij edit NIET verplicht.
            // Dit voorkomt de validation error als je het wachtwoordveld leeg laat.
            ModelState.Remove(nameof(Employee.PasswordHash));
            ModelState.Remove("PasswordHash");

            var existingEmployee = _employeeService.GetEmployees()
                .FirstOrDefault(e => e.EmployeeID == employee.EmployeeID);

            if (existingEmployee == null)
            {
                TempData[FlashErrorKey] = "Medewerker bestaat niet meer.";
                return RedirectToAction(nameof(Index));
            }

            // Status behouden. Actief/inactief aanpassen doe je via ToggleActive.
            employee.IsActive = existingEmployee.IsActive;

            // Leeg wachtwoord betekent: oude wachtwoord behouden.
            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                employee.PasswordHash = existingEmployee.PasswordHash;
            }

            ValidateEmployeeForEdit(employee);

            if (!ModelState.IsValid)
            {
                TempData[FlashErrorKey] = GetModelStateErrors();

                // Validatiefout: edit-form open houden
                return RedirectToAction(nameof(Index), new
                {
                    editId = employee.EmployeeID
                });
            }

            try
            {
                _employeeService.UpdateEmployee(employee);

                TempData[FlashSuccessKey] = "Medewerker bijgewerkt.";

                // Succes: form sluiten
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData[FlashErrorKey] = ex.Message;

                // Fout: edit-form open houden
                return RedirectToAction(nameof(Index), new
                {
                    editId = employee.EmployeeID
                });
            }
            catch
            {
                TempData[FlashErrorKey] = "Er is een onverwachte fout opgetreden bij het opslaan.";

                // Fout: edit-form open houden
                return RedirectToAction(nameof(Index), new
                {
                    editId = employee.EmployeeID
                });
            }
        }

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

        private void ValidateEmployeeForCreate(Employee employee)
        {
            var roles = GetRolesSafe();

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
        }

        private void ValidateEmployeeForEdit(Employee employee)
        {
            var roles = GetRolesSafe();

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
        }

        private List<Role> GetRolesSafe()
        {
            try
            {
                return _roleRepository.GetRoles();
            }
            catch
            {
                TempData[FlashErrorKey] = "Kon rollen niet ophalen. Controleer database/connectionstring.";
                return new List<Role>();
            }
        }

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