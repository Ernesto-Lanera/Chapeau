using System.Globalization;
using Chapeau.Constants;
using Chapeau.Repositories.Financial;

namespace Chapeau.Services
{
    public interface IFinancialService
    {
        FinancialOverviewData GetCurrentMonthFinancialData();
        FinancialOverviewData GetCurrentQuarterFinancialData();
        FinancialOverviewData GetCurrentYearFinancialData();
        FinancialOverviewData GetCustomRangeFinancialData(DateTime startDate, DateTime endDate);
    }

    public class FinancialService(IFinancialRepository financialRepository) : IFinancialService
    {
        private static readonly CultureInfo DutchCulture = CultureInfo.GetCultureInfo("nl-NL");
        private readonly IFinancialRepository _financialRepository = financialRepository;

        public FinancialOverviewData GetCurrentMonthFinancialData()
        {
            DateTime today = DateTime.Today;
            DateTime startDate = new(today.Year, today.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            return GetFinancialData(startDate, endDate, startDate.ToString("MMMM yyyy", DutchCulture));
        }

        public FinancialOverviewData GetCurrentQuarterFinancialData()
        {
            DateTime today = DateTime.Today;
            int quarter = ((today.Month - 1) / 3) + 1;
            DateTime startDate = new(today.Year, ((quarter - 1) * 3) + 1, 1);
            DateTime endDate = startDate.AddMonths(3).AddDays(-1);

            return GetFinancialData(startDate, endDate, $"Kwartaal {quarter} {today.Year}");
        }

        public FinancialOverviewData GetCurrentYearFinancialData()
        {
            DateTime today = DateTime.Today;
            DateTime startDate = new(today.Year, 1, 1);
            DateTime endDate = new(today.Year, 12, 31);

            return GetFinancialData(startDate, endDate, $"Jaar {today.Year}");
        }

        public FinancialOverviewData GetCustomRangeFinancialData(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date;

            if (startDate > endDate)
            {
                throw new ArgumentException("De begindatum mag niet na de einddatum liggen.");
            }

            string label = $"{startDate.ToString("d MMM yyyy", DutchCulture)} - {endDate.ToString("d MMM yyyy", DutchCulture)}";
            return GetFinancialData(startDate, endDate, label);
        }

        private FinancialOverviewData GetFinancialData(DateTime startDate, DateTime endDate, string periodDisplay)
        {
            List<MenuCardFinancialSummary> menuCardSummaries =
                _financialRepository.GetFinancialSummaryByMenuCard(startDate, endDate);

            MenuCardFinancialSummary drinks = FindMenuCardSummary(
                menuCardSummaries,
                MenuCardConstants.DrinksCardId,
                MenuCardConstants.DrinksCardName);

            MenuCardFinancialSummary lunch = FindMenuCardSummary(
                menuCardSummaries,
                MenuCardConstants.LunchCardId,
                MenuCardConstants.LunchCardName);

            MenuCardFinancialSummary dinner = FindMenuCardSummary(
                menuCardSummaries,
                MenuCardConstants.DinnerCardId,
                MenuCardConstants.DinnerCardName);

            List<RevenueTrendSummary> monthlyTrend = BuildMonthlyTrend(
                startDate,
                endDate,
                _financialRepository.GetMonthlyRevenueTrend(startDate, endDate));

            return new FinancialOverviewData
            {
                StartDate = startDate,
                EndDate = endDate,
                PeriodDisplay = periodDisplay,
                DrinksSalesCount = drinks.SalesCount,
                DrinksIncome = drinks.TotalIncome,
                LunchSalesCount = lunch.SalesCount,
                LunchIncome = lunch.TotalIncome,
                DinnerSalesCount = dinner.SalesCount,
                DinnerIncome = dinner.TotalIncome,
                TotalTips = _financialRepository.GetTotalTips(startDate, endDate),
                CategorySummaries = _financialRepository.GetFinancialSummaryByCategory(startDate, endDate),
                RevenueTrend = monthlyTrend
            };
        }

        private static MenuCardFinancialSummary FindMenuCardSummary(
            IEnumerable<MenuCardFinancialSummary> summaries,
            int menuCardId,
            string fallbackName)
        {
            return summaries.FirstOrDefault(summary => summary.MenuCardID == menuCardId)
                ?? new MenuCardFinancialSummary
                {
                    MenuCardID = menuCardId,
                    MenuCardName = fallbackName
                };
        }

        private static List<RevenueTrendSummary> BuildMonthlyTrend(
            DateTime startDate,
            DateTime endDate,
            IEnumerable<RevenueTrendSummary> databaseTrend)
        {
            Dictionary<DateTime, RevenueTrendSummary> valuesPerMonth = databaseTrend
                .ToDictionary(item => item.MonthStart.Date, item => item);

            var trend = new List<RevenueTrendSummary>();
            DateTime month = new(startDate.Year, startDate.Month, 1);
            DateTime finalMonth = new(endDate.Year, endDate.Month, 1);

            while (month <= finalMonth)
            {
                if (valuesPerMonth.TryGetValue(month, out RevenueTrendSummary? existingValue))
                {
                    trend.Add(existingValue);
                }
                else
                {
                    trend.Add(new RevenueTrendSummary { MonthStart = month });
                }

                month = month.AddMonths(1);
            }

            return trend;
        }
    }

    public class FinancialOverviewData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PeriodDisplay { get; set; } = string.Empty;

        public int DrinksSalesCount { get; set; }
        public decimal DrinksIncome { get; set; }

        public int LunchSalesCount { get; set; }
        public decimal LunchIncome { get; set; }

        public int DinnerSalesCount { get; set; }
        public decimal DinnerIncome { get; set; }

        public decimal TotalTips { get; set; }
        public List<CategoryFinancialSummary> CategorySummaries { get; set; } = new();
        public List<RevenueTrendSummary> RevenueTrend { get; set; } = new();

        public int TotalSalesCount => DrinksSalesCount + LunchSalesCount + DinnerSalesCount;
        public decimal TotalIncomeBeforeTips => DrinksIncome + LunchIncome + DinnerIncome;
        public decimal GrandTotalIncome => TotalIncomeBeforeTips + TotalTips;
    }
}
