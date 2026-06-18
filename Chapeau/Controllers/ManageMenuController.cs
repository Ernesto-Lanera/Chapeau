using Chapeau.Constants;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageMenuItems")]
    public class ManageMenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly ILogger<ManageMenuController> _logger;

        public ManageMenuController(IMenuService menuService, ILogger<ManageMenuController> logger)
        {
            _menuService = menuService;
            _logger = logger;
        }

        public IActionResult Index(int? cardId, int? categoryId, int? editId, bool showCreate = false)
        {
            MenuManagementViewModel viewModel = _menuService.GetManagementOverview(cardId, categoryId, editId, showCreate);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            MenuItemInputModel input, IFormFile? imageFile, int? returnCardId, int? returnCategoryId)
        {
            try
            {
                await _menuService.AddMenuItemAsync(input, imageFile);
                TempData[FlashMessages.SuccessKey] = "Menu-item is toegevoegd.";
            }
            catch (Exception exception)
            {
                return HandleMenuItemError(
                    exception,
                    "Onverwachte fout bij toevoegen van een menu-item.",
                    returnCardId,
                    returnCategoryId,
                    true,
                    null);
            }

            return RedirectToAction(nameof(Index), new { cardId = returnCardId, categoryId = returnCategoryId });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            MenuItemInputModel input, IFormFile? imageFile, int? returnCardId, int? returnCategoryId)
        {
            try
            {
                await _menuService.UpdateMenuItemAsync(input, imageFile);
                TempData[FlashMessages.SuccessKey] = "Menu-item is bijgewerkt.";
            }
            catch (Exception exception)
            {
                return HandleMenuItemError(
                    exception,
                    "Onverwachte fout bij bijwerken van een menu-item.",
                    returnCardId,
                    returnCategoryId,
                    false,
                    input.MenuItemID);
            }

            return RedirectToAction(nameof(Index), new { cardId = returnCardId, categoryId = returnCategoryId });
        }

        [HttpPost]
        public IActionResult ToggleActive(int id, bool active, int? cardId, int? categoryId)
        {
            try
            {
                _menuService.SetMenuItemActive(id, active);

                if (active)
                {
                    TempData[FlashMessages.SuccessKey] = "Menu-item geactiveerd.";
                }
                else
                {
                    TempData[FlashMessages.SuccessKey] = "Menu-item gedeactiveerd.";
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kon actieve status van menu-item niet wijzigen.");
                TempData[FlashMessages.ErrorKey] = ErrorMessages.UnexpectedError;
            }

            return RedirectToAction(nameof(Index), new { cardId, categoryId });
        }

        private IActionResult HandleMenuItemError(
            Exception exception,
            string logMessage,
            int? cardId,
            int? categoryId,
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
            return RedirectToAction(nameof(Index), new { cardId, categoryId, showCreate, editId });
        }
    }
}
