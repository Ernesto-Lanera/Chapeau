using System.Data;
using Chapeau.Constants;
using Chapeau.Emums;
using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories.Financial
{
    public class FinancialRepository(IConfiguration configuration, ILogger<FinancialRepository> logger) : IFinancialRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        private readonly ILogger<FinancialRepository> _logger = logger;

        public List<MenuCardFinancialSummary> GetFinancialSummaryByMenuCard(DateTime startDate, DateTime endDate)
        {
            const string query = """
                SELECT
                    c.MenuCardID,
                    mc.Name AS MenuCardName,
                    ISNULL(SUM(oi.AmountOrdered), 0) AS SalesCount,
                    ISNULL(SUM(CAST(oi.AmountOrdered AS DECIMAL(18, 2)) * m.Price), 0) AS TotalIncome
                FROM Orders AS o
                INNER JOIN OrderItem AS oi ON oi.OrderID = o.OrderID
                INNER JOIN MenuItems AS m ON m.MenuItemID = oi.MenuItemID
                INNER JOIN Categories AS c ON c.CategoryID = m.CategoryID
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                WHERE o.OrderStatus = @PaidStatus
                  AND o.OrderDate >= @StartDate
                  AND o.OrderDate < @EndDateExclusive
                GROUP BY c.MenuCardID, mc.Name
                ORDER BY c.MenuCardID;
                """;

            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(query, connection);
                AddPeriodParameters(command, startDate, endDate);

                connection.Open();

                var summaries = new List<MenuCardFinancialSummary>();
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    summaries.Add(new MenuCardFinancialSummary
                    {
                        MenuCardID = reader.GetInt32(reader.GetOrdinal("MenuCardID")),
                        MenuCardName = reader.GetString(reader.GetOrdinal("MenuCardName")),
                        SalesCount = Convert.ToInt32(reader["SalesCount"]),
                        TotalIncome = Convert.ToDecimal(reader["TotalIncome"])
                    });
                }

                return summaries;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Ophalen financiële totalen per menukaart is mislukt voor {StartDate} t/m {EndDate}.",
                    startDate,
                    endDate);

                throw;
            }
        }

        public decimal GetTotalTips(DateTime startDate, DateTime endDate)
        {
            const string query = """
                SELECT ISNULL(SUM(p.TotalTipAmount), 0) AS TotalTips
                FROM [Payment] AS p
                INNER JOIN Orders AS o ON o.OrderID = p.OrderID
                WHERE o.OrderStatus = @PaidStatus
                  AND o.OrderDate >= @StartDate
                  AND o.OrderDate < @EndDateExclusive;
                """;

            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(query, connection);
                AddPeriodParameters(command, startDate, endDate);

                connection.Open();

                object? value = command.ExecuteScalar();
                return value is null or DBNull ? 0m : Convert.ToDecimal(value);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Ophalen van fooien is mislukt voor {StartDate} t/m {EndDate}.",
                    startDate,
                    endDate);

                throw;
            }
        }

        public List<CategoryFinancialSummary> GetFinancialSummaryByCategory(DateTime startDate, DateTime endDate)
        {
            const string query = """
                SELECT
                    c.MenuCardID,
                    mc.Name AS MenuCardName,
                    c.Name AS CategoryName,
                    ISNULL(SUM(oi.AmountOrdered), 0) AS SalesCount,
                    ISNULL(SUM(CAST(oi.AmountOrdered AS DECIMAL(18, 2)) * m.Price), 0) AS TotalIncome
                FROM Orders AS o
                INNER JOIN OrderItem AS oi ON oi.OrderID = o.OrderID
                INNER JOIN MenuItems AS m ON m.MenuItemID = oi.MenuItemID
                INNER JOIN Categories AS c ON c.CategoryID = m.CategoryID
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                WHERE o.OrderStatus = @PaidStatus
                  AND o.OrderDate >= @StartDate
                  AND o.OrderDate < @EndDateExclusive
                GROUP BY c.MenuCardID, mc.Name, c.CategoryID, c.Name
                ORDER BY c.MenuCardID, TotalIncome DESC, c.Name;
                """;

            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(query, connection);
                AddPeriodParameters(command, startDate, endDate);

                connection.Open();

                var summaries = new List<CategoryFinancialSummary>();
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    summaries.Add(new CategoryFinancialSummary
                    {
                        MenuCardID = reader.GetInt32(reader.GetOrdinal("MenuCardID")),
                        MenuCardName = reader.GetString(reader.GetOrdinal("MenuCardName")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        SalesCount = Convert.ToInt32(reader["SalesCount"]),
                        TotalIncome = Convert.ToDecimal(reader["TotalIncome"])
                    });
                }

                return summaries;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Ophalen financiële totalen per categorie is mislukt voor {StartDate} t/m {EndDate}.",
                    startDate,
                    endDate);

                throw;
            }
        }

        public List<RevenueTrendSummary> GetMonthlyRevenueTrend(DateTime startDate, DateTime endDate)
        {
            const string query = """
                SELECT
                    DATEFROMPARTS(YEAR(o.OrderDate), MONTH(o.OrderDate), 1) AS MonthStart,
                    ISNULL(SUM(CASE
                        WHEN c.MenuCardID = @DrinksCardId
                        THEN CAST(oi.AmountOrdered AS DECIMAL(18, 2)) * m.Price
                        ELSE 0 END), 0) AS DrinksIncome,
                    ISNULL(SUM(CASE
                        WHEN c.MenuCardID = @LunchCardId
                        THEN CAST(oi.AmountOrdered AS DECIMAL(18, 2)) * m.Price
                        ELSE 0 END), 0) AS LunchIncome,
                    ISNULL(SUM(CASE
                        WHEN c.MenuCardID = @DinnerCardId
                        THEN CAST(oi.AmountOrdered AS DECIMAL(18, 2)) * m.Price
                        ELSE 0 END), 0) AS DinnerIncome
                FROM Orders AS o
                INNER JOIN OrderItem AS oi ON oi.OrderID = o.OrderID
                INNER JOIN MenuItems AS m ON m.MenuItemID = oi.MenuItemID
                INNER JOIN Categories AS c ON c.CategoryID = m.CategoryID
                WHERE o.OrderStatus = @PaidStatus
                  AND o.OrderDate >= @StartDate
                  AND o.OrderDate < @EndDateExclusive
                GROUP BY DATEFROMPARTS(YEAR(o.OrderDate), MONTH(o.OrderDate), 1)
                ORDER BY MonthStart;
                """;

            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = new(query, connection);
                AddPeriodParameters(command, startDate, endDate);
                command.Parameters.Add("@DrinksCardId", SqlDbType.Int).Value = MenuCardConstants.DrinksCardId;
                command.Parameters.Add("@LunchCardId", SqlDbType.Int).Value = MenuCardConstants.LunchCardId;
                command.Parameters.Add("@DinnerCardId", SqlDbType.Int).Value = MenuCardConstants.DinnerCardId;

                connection.Open();

                var trend = new List<RevenueTrendSummary>();
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    trend.Add(new RevenueTrendSummary
                    {
                        MonthStart = reader.GetDateTime(reader.GetOrdinal("MonthStart")),
                        DrinksIncome = Convert.ToDecimal(reader["DrinksIncome"]),
                        LunchIncome = Convert.ToDecimal(reader["LunchIncome"]),
                        DinnerIncome = Convert.ToDecimal(reader["DinnerIncome"])
                    });
                }

                return trend;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Ophalen omzettrend is mislukt voor {StartDate} t/m {EndDate}.",
                    startDate,
                    endDate);

                throw;
            }
        }

        private static void AddPeriodParameters(SqlCommand command, DateTime startDate, DateTime endDate)
        {
            command.Parameters.Add("@PaidStatus", SqlDbType.Int).Value = (int)OrderStatus.Paid;
            command.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value = startDate.Date;
            command.Parameters.Add("@EndDateExclusive", SqlDbType.DateTime2).Value = endDate.Date.AddDays(1);
        }
    }
}
