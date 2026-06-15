using Chapeau.Emums;
using Chapeau.Repositories;
using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    /// <summary>
    /// Manages the table overview grid, including occupancy toggling and marking orders as served.
    /// </summary>
    [Authorize(Policy = "CanTakeOrders")]
    public class TableController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ITableRepository _tableRepository;

        /// <summary>Initializes the controller with order and table repositories.</summary>
        public TableController(IOrderService orderService, ITableRepository tableRepository)
        {
            _orderService = orderService;
            _tableRepository = tableRepository;
        }

        /// <summary>Displays the table overview with all table statuses.</summary>
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

        /// <summary>Marks a single order as served.</summary>
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

        /// <summary>Marks all ready orders at a table as served.</summary>
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

        /// <summary>Toggles the manual occupied status of a table, blocking free if active orders exist.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int tableId, bool occupied, string? returnUrl = null, string? guestNames = null)
        {
            if (!occupied && _tableRepository.HasActiveOrders(tableId))
            {
                TempData["FlashError"] = "Deze tafel heeft nog openstaande bestellingen en kan niet worden vrijgemaakt.";
                return RedirectToAction(nameof(Index));
            }

            _tableRepository.SetOccupied(tableId, occupied);
            TempData["FlashSuccess"] = occupied ? "Tafel is gemarkeerd als bezet." : "Tafel is vrijgemaakt.";

            if (!string.IsNullOrEmpty(returnUrl))
            {
                if (!string.IsNullOrEmpty(guestNames))
                {
                    var separator = returnUrl.Contains('?') ? '&' : '?';
                    return Redirect(returnUrl + separator + "guestNames=" + Uri.EscapeDataString(guestNames));
                }
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
