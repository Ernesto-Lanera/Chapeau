using Chapeau.Constants;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageMenuItems")]
    public class ManageMenuController(IMenuService menuService, ILogger<ManageMenuController> logger) : Controller
    {
        private readonly IMenuService _menuService = menuService;
        private readonly ILogger<ManageMenuController> _logger = logger;

        public IActionResult Index(int? cardId, int? categoryId, int? editId, bool showCreate = false) =>
            View(_menuService.GetManagementOverview(cardId, categoryId, editId, showCreate));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MenuItemInputModel input, IFormFile? imageFile, int? returnCardId, int? returnCategoryId)
        {
            try
            {
                await _menuService.AddMenuItemAsync(input, imageFile);
                TempData[FlashMessages.SuccessKey] = $"Menu-item '{input.Name.Trim()}' is toegevoegd.";
                return RedirectToAction(nameof(Index), new { cardId = returnCardId, categoryId = returnCategoryId });
            }
            catch (Exception exception)
            {
                return HandleMenuItemError(
                    exception, 
                    "Onverwachte fout bij toevoegen van een menu-item.",
                    returnCardId, returnCategoryId, showCreate: true);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            MenuItemInputModel input, IFormFile? imageFile, int? returnCardId, int? returnCategoryId)
        {
            try
            {
                await _menuService.UpdateMenuItemAsync(input, imageFile);
                TempData[FlashMessages.SuccessKey] = $"Menu-item '{input.Name.Trim()}' is bijgewerkt.";
                return RedirectToAction(nameof(Index), new { cardId = returnCardId, categoryId = returnCategoryId });
            }
            catch (Exception exception)
            {
                return HandleMenuItemError(
                    exception,
                    $"Onverwachte fout bij bijwerken van menu-item {input.MenuItemID}.",
                    returnCardId, returnCategoryId, editId: input.MenuItemID);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int id, bool active, int? cardId, int? categoryId)
        {
            try
            {
                _menuService.SetMenuItemActive(id, active);
                TempData[FlashMessages.SuccessKey] = active
                    ? "Menu-item geactiveerd."
                    : "Menu-item gedeactiveerd.";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kon actieve status van menu-item {MenuItemId} niet wijzigen.", id);
                TempData[FlashMessages.ErrorKey] = ErrorMessages.UnexpectedError;
            }

            return RedirectToAction(nameof(Index), new { cardId, categoryId });
        }

        private IActionResult HandleMenuItemError(Exception exception,string logMessage,int? cardId,int? categoryId,bool showCreate = false,int? editId = null)
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
            return RedirectToAction(nameof(Index), new { cardId, categoryId, showCreate, editId });
        }
    }
}
