using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class FinancialViewModel
    {
        public FinancialPeriod SelectedPeriod { get; init; } = FinancialPeriod.Month;
        public DateTime? CustomStartDate { get; init; }
        public DateTime? CustomEndDate { get; init; }
        public string? FilterError { get; init; }

        public string PeriodDisplay { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

        public int DrinksSalesCount { get; init; }
        public decimal DrinksIncome { get; init; }
        public decimal DrinksAverageIncome => Average(DrinksIncome, DrinksSalesCount);

        public int LunchSalesCount { get; init; }
        public decimal LunchIncome { get; init; }
        public decimal LunchAverageIncome => Average(LunchIncome, LunchSalesCount);

        public int DinnerSalesCount { get; init; }
        public decimal DinnerIncome { get; init; }
        public decimal DinnerAverageIncome => Average(DinnerIncome, DinnerSalesCount);

        public decimal TotalTips { get; init; }
        public IReadOnlyList<CategoryFinancialSummary> CategorySummaries { get; init; } = Array.Empty<CategoryFinancialSummary>();
        public IReadOnlyList<RevenueTrendSummary> RevenueTrend { get; init; } = Array.Empty<RevenueTrendSummary>();

        public int TotalSalesCount => DrinksSalesCount + LunchSalesCount + DinnerSalesCount;
        public decimal TotalIncomeBeforeTips => DrinksIncome + LunchIncome + DinnerIncome;
        public decimal GrandTotalIncome => TotalIncomeBeforeTips + TotalTips;
        public decimal TotalAverageIncome => Average(TotalIncomeBeforeTips, TotalSalesCount);

        private static decimal Average(decimal total, int amount) => amount > 0 ? total / amount : 0m;
    }
}
