using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class FinancialViewModel
    {
        public FinancialPeriod SelectedPeriod { get; set; } = FinancialPeriod.Month;
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
        public string? FilterError { get; set; }

        public string PeriodDisplay { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int DrinksSalesCount { get; set; }
        public decimal DrinksIncome { get; set; }
        public decimal DrinksAverageIncome => Average(DrinksIncome, DrinksSalesCount);

        public int LunchSalesCount { get; set; }
        public decimal LunchIncome { get; set; }
        public decimal LunchAverageIncome => Average(LunchIncome, LunchSalesCount);

        public int DinnerSalesCount { get; set; }
        public decimal DinnerIncome { get; set; }
        public decimal DinnerAverageIncome => Average(DinnerIncome, DinnerSalesCount);

        public decimal TotalTips { get; set; }
        public List<RevenueTrendSummary> RevenueTrend { get; set; } = new List<RevenueTrendSummary>();
        public int TotalSalesCount => DrinksSalesCount + LunchSalesCount + DinnerSalesCount;
        public decimal TotalIncomeBeforeTips => DrinksIncome + LunchIncome + DinnerIncome;
        public decimal TotalAverageIncome => Average(TotalIncomeBeforeTips, TotalSalesCount);

        private static decimal Average(decimal total, int amount)
        {
            if (amount <= 0)
            {
                return 0m;
            }

            return total / amount;
        }
    }
}
