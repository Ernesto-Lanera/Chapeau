using Chapeau.Repositories.Financial;

namespace Chapeau.ViewModels
{
    public class FinancialOverviewViewModel
    {
        public string SelectedPeriod { get; set; } = "month";
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
        public string? FilterError { get; set; }

        public string PeriodDisplay { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

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
