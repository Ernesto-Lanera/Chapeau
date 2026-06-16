using Chapeau.Constants;
using Chapeau.Services.Overview;
using Chapeau.Extensions;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageEmployees")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        public IActionResult Index(int? editId, bool showCreate = false)
        {
            EmployeeManagementViewModel viewModel = _employeeService.GetManagementOverview(editId, showCreate);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(EmployeeInputModel input)
        {
            try
            {
                _employeeService.AddEmployee(input);
                TempData[FlashMessages.SuccessKey] = "Medewerker toegevoegd.";
            }
            catch (Exception exception)
            {
                return HandleEmployeeError(
                    exception,
                    "Onverwachte fout bij toevoegen van een medewerker.",
                    true,
                    null);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Edit(EmployeeInputModel input)
        {
            try
            {
                _employeeService.UpdateEmployee(input);
                TempData[FlashMessages.SuccessKey] = "Medewerker bijgewerkt.";
            }
            catch (Exception exception)
            {
                return HandleEmployeeError(
                    exception,
                    "Onverwachte fout bij bijwerken van een medewerker.",
                    false,
                    input.EmployeeID);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
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

                if (active)
                {
                    TempData[FlashMessages.SuccessKey] = "Medewerker geactiveerd. Deze medewerker kan weer inloggen.";
                }
                else
                {
                    TempData[FlashMessages.SuccessKey] = "Medewerker gedeactiveerd. Deze medewerker kan niet meer inloggen.";
                }
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
                _logger.LogError(exception, "Kon actieve status van medewerker niet wijzigen.");
                TempData[FlashMessages.ErrorKey] = ErrorMessages.UnexpectedError;
            }

            return RedirectToAction(nameof(Index));
        }

        private IActionResult HandleEmployeeError(
            Exception exception,
            string logMessage,
            bool showCreate,
            int? editId)
        {
            string errorMessage = ErrorMessages.UnexpectedError;

            if (exception is ArgumentException || exception is InvalidOperationException)
            {
                errorMessage = exception.Message;
            }
            else
            {
                _logger.LogError(exception, logMessage);
            }

            TempData[FlashMessages.ErrorKey] = errorMessage;
            return RedirectToAction(nameof(Index), new { showCreate, editId });
        }
    }
}
