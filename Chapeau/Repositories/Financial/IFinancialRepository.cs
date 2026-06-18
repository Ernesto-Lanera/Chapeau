using Chapeau.Models;

namespace Chapeau.Repositories.Financial
{
    public interface IFinancialRepository
    {
        List<MenuCardFinancialSummary> GetFinancialSummaryByMenuCard(DateTime startDate, DateTime endDate);
        decimal GetTotalTips(DateTime startDate, DateTime endDate);
        List<RevenueTrendSummary> GetMonthlyRevenueTrend(DateTime startDate, DateTime endDate);
    }
}
