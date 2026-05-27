using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageStock")]
    public class StockController(IStockService stockService, ILogger<StockController> logger) : Controller
    {
        private readonly IStockService _stockService = stockService;
        private readonly ILogger<StockController> _logger = logger;

        public IActionResult Index(int? cardId, int? categoryId) =>
            View(_stockService.GetOverview(cardId, categoryId));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, int newStock, int? cardId, int? categoryId)
        {
            try
            {
                MenuItem updatedItem = _stockService.ChangeStock(id, newStock);
                if (IsAjaxRequest())
                {
                    StockOverviewViewModel overview = _stockService.GetOverview(cardId, categoryId);
                    return Ok(new
                    {
                        success = true,
                        updatedItem.Stock,
                        statusText = updatedItem.StockStatusText,
                        statusCssClass = updatedItem.StockStatusCssClass,
                        updatedItem.IsOutOfStock,
                        soldOutCount = overview.SoldOutCount,
                        almostOutCount = overview.AlmostOutCount
                    });
                }

                TempData[FlashMessages.SuccessKey] = "Voorraad succesvol bijgewerkt.";
            }
            catch (ArgumentException exception)
            {
                return HandleValidationFailure(exception.Message, cardId, categoryId);
            }
            catch (InvalidOperationException exception)
            {
                return HandleValidationFailure(exception.Message, cardId, categoryId);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Onverwachte fout bij voorraadwijziging voor menu-item {MenuItemId}.", id);
                return HandleValidationFailure(ErrorMessages.UnexpectedError, cardId, categoryId);
            }

            return RedirectToAction(nameof(Index), new { cardId, categoryId });
        }

        private IActionResult HandleValidationFailure(string message, int? cardId, int? categoryId)
        {
            if (IsAjaxRequest()) return BadRequest(new { success = false, message });

            TempData[FlashMessages.ErrorKey] = message;
            return RedirectToAction(nameof(Index), new { cardId, categoryId });
        }

        private bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "fetch";
    }
}
