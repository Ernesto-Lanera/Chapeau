using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanViewFinance")]
    public class FinanceController : Controller
    {
        private readonly IFinancialService _financialService;

        public FinanceController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpGet]
        public IActionResult Index(string? period = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            FinancialViewModel viewModel = _financialService.GetOverview(period, startDate, endDate);
            return View(viewModel);
        }
    }
}
