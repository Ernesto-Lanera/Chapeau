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
            MenuItemInputModel input, IFormFile? imageFile, int? cardId, int? categoryId)
        {
            try
            {
                await _menuService.AddMenuItemAsync(input, imageFile);
                TempData[FlashMessages.SuccessKey] = $"Menu-item '{input.Name.Trim()}' is toegevoegd.";
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
            catch (ArgumentException exception)
            {
                return RedirectWithError(exception.Message, cardId, categoryId, showCreate: true);
            }
            catch (InvalidOperationException exception)
            {
                return RedirectWithError(exception.Message, cardId, categoryId, showCreate: true);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Onverwachte fout bij toevoegen van een menu-item.");
                return RedirectWithError(ErrorMessages.UnexpectedError, cardId, categoryId, showCreate: true);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            MenuItemInputModel input, IFormFile? imageFile, int? cardId, int? categoryId)
        {
            try
            {
                await _menuService.UpdateMenuItemAsync(input, imageFile);
                TempData[FlashMessages.SuccessKey] = $"Menu-item '{input.Name.Trim()}' is bijgewerkt.";
                return RedirectToAction(nameof(Index), new { cardId, categoryId });
            }
            catch (ArgumentException exception)
            {
                return RedirectWithError(exception.Message, cardId, categoryId, editId: input.MenuItemID);
            }
            catch (InvalidOperationException exception)
            {
                return RedirectWithError(exception.Message, cardId, categoryId, editId: input.MenuItemID);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Onverwachte fout bij bijwerken van menu-item {MenuItemId}.", input.MenuItemID);
                return RedirectWithError(ErrorMessages.UnexpectedError, cardId, categoryId, editId: input.MenuItemID);
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

        private IActionResult RedirectWithError(
            string message, int? cardId, int? categoryId, bool showCreate = false, int? editId = null)
        {
            TempData[FlashMessages.ErrorKey] = message;
            return RedirectToAction(nameof(Index), new { cardId, categoryId, showCreate, editId });
        }
    }
}
