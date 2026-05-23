namespace Chapeau.Repositories.Financial
{
    public interface IFinancialRepository
    {
        /// <summary>
        /// Returns the quantity sold and generated income per menu card in a paid-order period.
        /// </summary>
        List<MenuCardFinancialSummary> GetFinancialSummaryByMenuCard(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns the sum of non-cash tips registered on payments for paid orders in a period.
        /// </summary>
        decimal GetTotalTips(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns the quantity sold and generated income per category in a paid-order period.
        /// </summary>
        List<CategoryFinancialSummary> GetFinancialSummaryByCategory(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns monthly income totals per menu card for the omzet trend chart.
        /// </summary>
        List<RevenueTrendSummary> GetMonthlyRevenueTrend(DateTime startDate, DateTime endDate);
    }

    public class MenuCardFinancialSummary
    {
        public int MenuCardID { get; set; }
        public string MenuCardName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal TotalIncome { get; set; }
    }

    public class CategoryFinancialSummary
    {
        public int MenuCardID { get; set; }
        public string MenuCardName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal AveragePerSoldItem => SalesCount > 0 ? TotalIncome / SalesCount : 0m;
    }

    public class RevenueTrendSummary
    {
        public DateTime MonthStart { get; set; }
        public decimal DrinksIncome { get; set; }
        public decimal LunchIncome { get; set; }
        public decimal DinnerIncome { get; set; }
    }
}
