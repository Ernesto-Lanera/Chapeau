using Chapeau.Emums;
using Chapeau.Repositories;
using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanTakeOrders")]
    public class TableController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly TableRepository _tableRepository;

        public TableController(IOrderService orderService, TableRepository tableRepository)
        {
            _orderService = orderService;
            _tableRepository = tableRepository;
        }
        public IActionResult Index()
        {
            try
            {
                var tables = _orderService.GetAllTableStatuses();
                return View(tables);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsServed(int orderId)
        {
            try
            {
                _orderService.MarkOrderAsServed(orderId);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkTableServed(int tableId)
        {
            try
            {
                int updated = _orderService.MarkTableServed(tableId);
                if (updated > 0)
                {
                    TempData["FlashSuccess"] = $"Bestellingen voor tafel {tableId} zijn gemarkeerd als geserveerd.";
                }
                else
                {
                    TempData["FlashError"] = "Er zijn geen bestellingen gevonden die geserveerd kunnen worden.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["FlashError"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int tableId, bool occupied, string? returnUrl = null)
        {
            if (!occupied && _tableRepository.HasActiveOrders(tableId))
            {
                TempData["FlashError"] = "Deze tafel heeft nog openstaande bestellingen en kan niet worden vrijgemaakt.";
                return RedirectToAction(nameof(Index));
            }

            _tableRepository.SetOccupied(tableId, occupied);
            TempData["FlashSuccess"] = occupied ? "Tafel is gemarkeerd als bezet." : "Tafel is vrijgemaakt.";

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}
