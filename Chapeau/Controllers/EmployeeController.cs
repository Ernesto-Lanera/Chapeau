using Chapeau.Constants;
using Chapeau.Services.Overview;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageEmployees")]
    public class EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger) : Controller
    {
        private readonly IEmployeeService _employeeService = employeeService;
        private readonly ILogger<EmployeeController> _logger = logger;

        public IActionResult Index(int? editId, bool showCreate = false) =>
            View(_employeeService.GetManagementOverview(editId, showCreate));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EmployeeInputModel input)
        {
            try
            {
                _employeeService.AddEmployee(input);
                TempData[FlashMessages.SuccessKey] = "Medewerker toegevoegd.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException exception)
            {
                return RedirectWithError(exception.Message, showCreate: true);
            }
            catch (InvalidOperationException exception)
            {
                return RedirectWithError(exception.Message, showCreate: true);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Onverwachte fout bij toevoegen van een medewerker.");
                return RedirectWithError(ErrorMessages.UnexpectedError, showCreate: true);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EmployeeInputModel input)
        {
            try
            {
                _employeeService.UpdateEmployee(input);
                TempData[FlashMessages.SuccessKey] = "Medewerker bijgewerkt.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException exception)
            {
                return RedirectWithError(exception.Message, editId: input.EmployeeID);
            }
            catch (InvalidOperationException exception)
            {
                return RedirectWithError(exception.Message, editId: input.EmployeeID);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Onverwachte fout bij bijwerken van medewerker {EmployeeId}.", input.EmployeeID);
                return RedirectWithError(ErrorMessages.UnexpectedError, editId: input.EmployeeID);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int id, bool active)
        {
            try
            {
                _employeeService.SetEmployeeActive(id, active);
                TempData[FlashMessages.SuccessKey] = active
                    ? "Medewerker geactiveerd. Deze medewerker kan weer inloggen."
                    : "Medewerker gedeactiveerd. Deze medewerker kan niet meer inloggen.";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kon actieve status van medewerker {EmployeeId} niet wijzigen.", id);
                TempData[FlashMessages.ErrorKey] = ErrorMessages.UnexpectedError;
            }

            return RedirectToAction(nameof(Index));
        }

        private IActionResult RedirectWithError(string message, bool showCreate = false, int? editId = null)
        {
            TempData[FlashMessages.ErrorKey] = message;
            return RedirectToAction(nameof(Index), new { showCreate, editId });
        }
    }
}
