using Chapeau.Constants;
using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanManageStock")]
    public class StockController : Controller
    {
        private readonly IStockService _stockService;
        private readonly ILogger<StockController> _logger;

        public StockController(IStockService stockService, ILogger<StockController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        public IActionResult Index(int? cardId, int? categoryId)
        {
            StockOverviewViewModel viewModel = _stockService.GetOverview(cardId, categoryId);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UpdateDirect(int id, int newStock)
        {
            try
            {
                var item = _stockService.ChangeStock(id, newStock);

                return Json(new
                {
                    success = true,
                    stock = item.Stock,
                    statusText = item.StockDisplayStatusText,
                    statusClass = item.StockDisplayStatusCssClass,
                    rowClass = item.StockRowCssClass
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { success = false, message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { success = false, message = exception.Message });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Onverwachte fout bij directe voorraadwijziging.");
                return BadRequest(new { success = false, message = ErrorMessages.UnexpectedError });
            }
        }
    }
}
