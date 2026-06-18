namespace Chapeau.Models
{
    public class MenuCardFinancialSummary
    {
        public int MenuCardID { get; set; }
        public string MenuCardName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal TotalIncome { get; set; }

        public decimal AverageIncomePerItem => SalesCount > 0 ? TotalIncome / SalesCount : 0m;
    }

    public class RevenueTrendSummary
    {
        public DateTime MonthStart { get; set; }
        public decimal DrinksIncome { get; set; }
        public decimal LunchIncome { get; set; }
        public decimal DinnerIncome { get; set; }
    }
}
