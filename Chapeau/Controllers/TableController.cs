using Chapeau.Emums;
using Chapeau.Repositories;
using Chapeau.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
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
            var tables = _orderService.GetAllTableStatuses();
            return View(tables);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsServed(int orderId)
        {
            _orderService.MarkOrderAsServed(orderId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int tableId, bool occupied)
        {
            if (!occupied && _tableRepository.HasActiveOrders(tableId))
            {
                TempData["FlashError"] = "Deze tafel heeft nog openstaande bestellingen en kan niet worden vrijgemaakt.";
                return RedirectToAction(nameof(Index));
            }

            _tableRepository.SetOccupied(tableId, occupied);
            TempData["FlashSuccess"] = occupied ? "Tafel is gemarkeerd als bezet." : "Tafel is vrijgemaakt.";
            return RedirectToAction(nameof(Index));
        }
    }
}
