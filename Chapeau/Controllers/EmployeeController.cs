using Chapeau.Constants;
using Chapeau.Services.Overview;
using Chapeau.Extensions;
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
            catch (Exception exception)
            {
                return HandleEmployeeError(
                    exception,
                    "Onverwachte fout bij toevoegen van een medewerker.",
                    showCreate: true);
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
            catch (Exception exception)
            {
                return HandleEmployeeError(
                    exception,
                    $"Onverwachte fout bij bijwerken van medewerker {input.EmployeeID}.",
                    editId: input.EmployeeID);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int id, bool active)
        {
            try
            {
                if (!active && id == User.GetEmployeeId())
                {
                    TempData[FlashMessages.ErrorKey] = "Je kunt je eigen account niet deactiveren.";
                    return RedirectToAction(nameof(Index));
                }

                _employeeService.SetEmployeeActive(id, active);
                TempData[FlashMessages.SuccessKey] = active
                    ? "Medewerker geactiveerd. Deze medewerker kan weer inloggen."
                    : "Medewerker gedeactiveerd. Deze medewerker kan niet meer inloggen.";
            }
            catch (ArgumentException exception)
            {
                TempData[FlashMessages.ErrorKey] = exception.Message;
            }
            catch (InvalidOperationException exception)
            {
                TempData[FlashMessages.ErrorKey] = exception.Message;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kon actieve status van medewerker {EmployeeId} niet wijzigen.", id);
                TempData[FlashMessages.ErrorKey] = ErrorMessages.UnexpectedError;
            }

            return RedirectToAction(nameof(Index));
        }

        private IActionResult HandleEmployeeError(
            Exception exception,
            string logMessage,
            bool showCreate = false,
            int? editId = null)
        {
            string errorMessage = exception switch
            {
                ArgumentException => exception.Message,
                InvalidOperationException => exception.Message,
                _ => ErrorMessages.UnexpectedError
            };

            if (exception is not (ArgumentException or InvalidOperationException))
            {
                _logger.LogError(exception, logMessage);
            }

            TempData[FlashMessages.ErrorKey] = errorMessage;
            return RedirectToAction(nameof(Index), new { showCreate, editId });
        }
    }
}
