using Chapeau.Services;
using Chapeau.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau.Controllers
{
    [Authorize(Policy = "CanViewReports")]
    public class FinanceController(IFinancialService financialService) : Controller
    {
        private readonly IFinancialService _financialService = financialService;

        [HttpGet]
        public IActionResult Index(string period = "month", DateTime? startDate = null, DateTime? endDate = null)
        {
            period = NormalizePeriod(period);

            string? filterError = null;
            FinancialOverviewData financialData;

            if (period == "custom")
            {
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    filterError = "Kies zowel een begindatum als een einddatum. De gegevens van deze maand worden getoond.";
                    financialData = _financialService.GetCurrentMonthFinancialData();
                }
                else if (startDate.Value.Date > endDate.Value.Date)
                {
                    filterError = "De begindatum mag niet na de einddatum liggen. De gegevens van deze maand worden getoond.";
                    financialData = _financialService.GetCurrentMonthFinancialData();
                }
                else
                {
                    financialData = _financialService.GetCustomRangeFinancialData(startDate.Value, endDate.Value);
                }
            }
            else
            {
                financialData = period switch
                {
                    "quarter" => _financialService.GetCurrentQuarterFinancialData(),
                    "year" => _financialService.GetCurrentYearFinancialData(),
                    _ => _financialService.GetCurrentMonthFinancialData()
                };
            }

            return View(new FinancialOverviewViewModel
            {
                SelectedPeriod = period,
                CustomStartDate = startDate,
                CustomEndDate = endDate,
                FilterError = filterError,
                PeriodDisplay = financialData.PeriodDisplay,
                StartDate = financialData.StartDate,
                EndDate = financialData.EndDate,
                DrinksSalesCount = financialData.DrinksSalesCount,
                DrinksIncome = financialData.DrinksIncome,
                LunchSalesCount = financialData.LunchSalesCount,
                LunchIncome = financialData.LunchIncome,
                DinnerSalesCount = financialData.DinnerSalesCount,
                DinnerIncome = financialData.DinnerIncome,
                TotalTips = financialData.TotalTips,
                CategorySummaries = financialData.CategorySummaries,
                RevenueTrend = financialData.RevenueTrend
            });
        }

        private static string NormalizePeriod(string? period)
        {
            return period?.ToLowerInvariant() switch
            {
                "quarter" => "quarter",
                "year" => "year",
                "custom" => "custom",
                _ => "month"
            };
        }
    }
}
