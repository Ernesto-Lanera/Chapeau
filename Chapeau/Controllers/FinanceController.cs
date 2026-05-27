using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanViewFinance")]
    public class FinanceController(IFinancialService financialService) : Controller
    {
        private readonly IFinancialService _financialService = financialService;

        [HttpGet]
        public IActionResult Index(string? period = null, DateTime? startDate = null, DateTime? endDate = null) =>
            View(_financialService.GetOverview(period, startDate, endDate));
    }
}
