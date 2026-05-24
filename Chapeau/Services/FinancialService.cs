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

    public class FinancialService(IFinancialRepository financialRepository) : IFinancialService
    {
        private static readonly CultureInfo DutchCulture = CultureInfo.GetCultureInfo("nl-NL");
        private readonly IFinancialRepository _financialRepository = financialRepository;

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
                CategorySummaries = _financialRepository.GetFinancialSummaryByCategory(range.firstDay, range.lastDay),
                RevenueTrend = CompleteMonthlyTrend(
                    range.firstDay,
                    range.lastDay,
                    _financialRepository.GetMonthlyRevenueTrend(range.firstDay, range.lastDay))
            };
        }

        private static FinancialPeriod ParsePeriod(string? period) => period?.ToLowerInvariant() switch
        {
            "quarter" => FinancialPeriod.Quarter,
            "year" => FinancialPeriod.Year,
            "custom" => FinancialPeriod.Custom,
            _ => FinancialPeriod.Month
        };

        private static (DateTime firstDay, DateTime lastDay, string display) GetRange(
            FinancialPeriod period, DateTime? startDate, DateTime? endDate) => period switch
        {
            FinancialPeriod.Quarter => GetQuarterRange(DateTime.Today),
            FinancialPeriod.Year => GetYearRange(DateTime.Today),
            FinancialPeriod.Custom => GetCustomRange(startDate!.Value, endDate!.Value),
            _ => GetMonthRange(DateTime.Today)
        };

        private static (DateTime, DateTime, string) GetMonthRange(DateTime today)
        {
            DateTime start = new(today.Year, today.Month, 1);
            return (start, start.AddMonths(1).AddDays(-1), start.ToString("MMMM yyyy", DutchCulture));
        }

        private static (DateTime, DateTime, string) GetQuarterRange(DateTime today)
        {
            int quarter = ((today.Month - 1) / 3) + 1;
            DateTime start = new(today.Year, ((quarter - 1) * 3) + 1, 1);
            return (start, start.AddMonths(3).AddDays(-1), $"Kwartaal {quarter} {today.Year}");
        }

        private static (DateTime, DateTime, string) GetYearRange(DateTime today)
        {
            DateTime start = new(today.Year, 1, 1);
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

        private static IReadOnlyList<RevenueTrendSummary> CompleteMonthlyTrend(
            DateTime startDate, DateTime endDate, IEnumerable<RevenueTrendSummary> databaseTrend)
        {
            Dictionary<DateTime, RevenueTrendSummary> values =
                databaseTrend.ToDictionary(item => item.MonthStart.Date, item => item);
            var result = new List<RevenueTrendSummary>();

            for (DateTime month = new(startDate.Year, startDate.Month, 1);
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
