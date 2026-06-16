using System.Globalization;
using Chapeau.Constants;
using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Repositories.Financial;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public interface IFinancialService
    {
        FinancialViewModel GetOverview(string? period, DateTime? startDate, DateTime? endDate);
    }

    public class FinancialService : IFinancialService
    {
        private static readonly CultureInfo DutchCulture = CultureInfo.GetCultureInfo("nl-NL");
        private readonly IFinancialRepository _financialRepository;

        public FinancialService(IFinancialRepository financialRepository)
        {
            _financialRepository = financialRepository;
        }

        public FinancialViewModel GetOverview(string? period, DateTime? startDate, DateTime? endDate)
        {
            FinancialPeriod selectedPeriod = ParsePeriod(period);
            string? error = null;
            (DateTime firstDay, DateTime lastDay, string display) range;

            if (selectedPeriod == FinancialPeriod.Custom && (!startDate.HasValue || !endDate.HasValue))
            {
                error = "Kies zowel een begindatum als een einddatum. De gegevens van deze maand worden getoond.";
                range = GetMonthRange(DateTime.Today);
            }
            else if (selectedPeriod == FinancialPeriod.Custom && startDate!.Value.Date > endDate!.Value.Date)
            {
                error = "De begindatum mag niet na de einddatum liggen. De gegevens van deze maand worden getoond.";
                range = GetMonthRange(DateTime.Today);
            }
            else
            {
                range = GetRange(selectedPeriod, startDate, endDate);
            }

            return CreateViewModel(selectedPeriod, startDate, endDate, error, range);
        }

        private FinancialViewModel CreateViewModel(
            FinancialPeriod selectedPeriod,
            DateTime? customStartDate,
            DateTime? customEndDate,
            string? error,
            (DateTime firstDay, DateTime lastDay, string display) range)
        {
            List<MenuCardFinancialSummary> cards =
                _financialRepository.GetFinancialSummaryByMenuCard(range.firstDay, range.lastDay);

            MenuCardFinancialSummary drinks = Find(cards, MenuCardConstants.DrinksCardId, MenuCardConstants.DrinksCardName);
            MenuCardFinancialSummary lunch = Find(cards, MenuCardConstants.LunchCardId, MenuCardConstants.LunchCardName);
            MenuCardFinancialSummary dinner = Find(cards, MenuCardConstants.DinnerCardId, MenuCardConstants.DinnerCardName);

            return new FinancialViewModel
            {
                SelectedPeriod = selectedPeriod,
                CustomStartDate = customStartDate,
                CustomEndDate = customEndDate,
                FilterError = error,
                PeriodDisplay = range.display,
                StartDate = range.firstDay,
                EndDate = range.lastDay,
                DrinksSalesCount = drinks.SalesCount,
                DrinksIncome = drinks.TotalIncome,
                LunchSalesCount = lunch.SalesCount,
                LunchIncome = lunch.TotalIncome,
                DinnerSalesCount = dinner.SalesCount,
                DinnerIncome = dinner.TotalIncome,
                TotalTips = _financialRepository.GetTotalTips(range.firstDay, range.lastDay),
                RevenueTrend = CompleteMonthlyTrend(
                    range.firstDay,
                    range.lastDay,
                    _financialRepository.GetMonthlyRevenueTrend(range.firstDay, range.lastDay))
            };
        }

        private static FinancialPeriod ParsePeriod(string? period)
        {
            if (string.IsNullOrWhiteSpace(period))
            {
                return FinancialPeriod.Month;
            }

            string value = period.ToLowerInvariant();

            if (value == "quarter")
            {
                return FinancialPeriod.Quarter;
            }

            if (value == "year")
            {
                return FinancialPeriod.Year;
            }

            if (value == "custom")
            {
                return FinancialPeriod.Custom;
            }

            return FinancialPeriod.Month;
        }

        private static (DateTime firstDay, DateTime lastDay, string display) GetRange(
            FinancialPeriod period, DateTime? startDate, DateTime? endDate)
        {
            if (period == FinancialPeriod.Quarter)
            {
                return GetQuarterRange(DateTime.Today);
            }

            if (period == FinancialPeriod.Year)
            {
                return GetYearRange(DateTime.Today);
            }

            if (period == FinancialPeriod.Custom)
            {
                return GetCustomRange(startDate!.Value, endDate!.Value);
            }

            return GetMonthRange(DateTime.Today);
        }

        private static (DateTime, DateTime, string) GetMonthRange(DateTime today)
        {
            DateTime start = new DateTime(today.Year, today.Month, 1);
            return (start, start.AddMonths(1).AddDays(-1), start.ToString("MMMM yyyy", DutchCulture));
        }

        private static (DateTime, DateTime, string) GetQuarterRange(DateTime today)
        {
            int quarter = ((today.Month - 1) / 3) + 1;
            DateTime start = new DateTime(today.Year, ((quarter - 1) * 3) + 1, 1);
            return (start, start.AddMonths(3).AddDays(-1), $"Kwartaal {quarter} {today.Year}");
        }

        private static (DateTime, DateTime, string) GetYearRange(DateTime today)
        {
            DateTime start = new DateTime(today.Year, 1, 1);
            return (start, new DateTime(today.Year, 12, 31), $"Jaar {today.Year}");
        }

        private static (DateTime, DateTime, string) GetCustomRange(DateTime startDate, DateTime endDate)
        {
            DateTime start = startDate.Date;
            DateTime end = endDate.Date;
            return (start, end, $"{start.ToString("d MMM yyyy", DutchCulture)} - {end.ToString("d MMM yyyy", DutchCulture)}");
        }

        private static MenuCardFinancialSummary Find(
            IEnumerable<MenuCardFinancialSummary> summaries, int menuCardId, string name) =>
            summaries.FirstOrDefault(summary => summary.MenuCardID == menuCardId)
            ?? new MenuCardFinancialSummary { MenuCardID = menuCardId, MenuCardName = name };

        private static List<RevenueTrendSummary> CompleteMonthlyTrend(
            DateTime startDate, DateTime endDate, IEnumerable<RevenueTrendSummary> databaseTrend)
        {
            Dictionary<DateTime, RevenueTrendSummary> values =
                databaseTrend.ToDictionary(item => item.MonthStart.Date, item => item);
            List<RevenueTrendSummary> result = new List<RevenueTrendSummary>();

            for (DateTime month = new DateTime(startDate.Year, startDate.Month, 1);
                 month <= new DateTime(endDate.Year, endDate.Month, 1);
                 month = month.AddMonths(1))
            {
                result.Add(values.TryGetValue(month, out RevenueTrendSummary? value)
                    ? value
                    : new RevenueTrendSummary { MonthStart = month });
            }

            return result;
        }
    }
}
