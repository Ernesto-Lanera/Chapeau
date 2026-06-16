using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories.Financial
{
    public class FinancialRepository : IFinancialRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<FinancialRepository> _logger;

        public FinancialRepository(IConfiguration configuration, ILogger<FinancialRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
            _logger = logger;
        }

        public List<MenuCardFinancialSummary> GetFinancialSummaryByMenuCard(DateTime startDate, DateTime endDate)
        {
            return ReadRows(
                SummaryByMenuCardQuery,
                startDate,
                endDate,
                MapMenuCardSummary,
                "Ophalen financiële totalen per menukaart is mislukt");
        }

        public decimal GetTotalTips(DateTime startDate, DateTime endDate)
        {
            return ReadDecimal(
                TotalTipsQuery,
                startDate,
                endDate,
                "Ophalen van fooien is mislukt");
        }

        public List<RevenueTrendSummary> GetMonthlyRevenueTrend(DateTime startDate, DateTime endDate)
        {
            return ReadRows(
                MonthlyTrendQuery,
                startDate,
                endDate,
                MapRevenueTrend,
                "Ophalen omzettrend is mislukt",
                AddMenuCardParameters);
        }

        private List<T> ReadRows<T>(
            string query,
            DateTime startDate,
            DateTime endDate,
            Func<SqlDataReader, T> mapRow,
            string errorMessage,
            Action<SqlCommand>? addExtraParameters = null)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = CreatePeriodCommand(connection, query, startDate, endDate);
                addExtraParameters?.Invoke(command);

                connection.Open();

                List<T> rows = new List<T>();
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    rows.Add(mapRow(reader));
                }

                return rows;
            }
            catch (Exception exception)
            {
                LogFinancialError(exception, errorMessage, startDate, endDate);
                throw;
            }
        }

        private decimal ReadDecimal(string query, DateTime startDate, DateTime endDate, string errorMessage)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = CreatePeriodCommand(connection, query, startDate, endDate);

                connection.Open();

                object? value = command.ExecuteScalar();
                return value is null or DBNull ? 0m : Convert.ToDecimal(value);
            }
            catch (Exception exception)
            {
                LogFinancialError(exception, errorMessage, startDate, endDate);
                throw;
            }
        }

        private static SqlCommand CreatePeriodCommand(
            SqlConnection connection,
            string query,
            DateTime startDate,
            DateTime endDate)
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value = startDate.Date;
            command.Parameters.Add("@EndDateExclusive", SqlDbType.DateTime2).Value = endDate.Date.AddDays(1);

            return command;
        }

        private static void AddMenuCardParameters(SqlCommand command)
        {
            command.Parameters.Add("@DrinksCardId", SqlDbType.Int).Value = MenuCardConstants.DrinksCardId;
            command.Parameters.Add("@LunchCardId", SqlDbType.Int).Value = MenuCardConstants.LunchCardId;
            command.Parameters.Add("@DinnerCardId", SqlDbType.Int).Value = MenuCardConstants.DinnerCardId;
        }

        private void LogFinancialError(Exception exception, string errorMessage, DateTime startDate, DateTime endDate)
        {
            _logger.LogError(
                exception,
                "{ErrorMessage} voor {StartDate} t/m {EndDate}.",
                errorMessage,
                startDate,
                endDate);
        }

        private static MenuCardFinancialSummary MapMenuCardSummary(SqlDataReader reader)
        {
            return new MenuCardFinancialSummary
            {
                MenuCardID = GetInt(reader, "MenuCardID"),
                MenuCardName = GetString(reader, "MenuCardName"),
                SalesCount = GetInt(reader, "SalesCount"),
                TotalIncome = GetDecimal(reader, "TotalIncome")
            };
        }

        private static RevenueTrendSummary MapRevenueTrend(SqlDataReader reader)
        {
            return new RevenueTrendSummary
            {
                MonthStart = reader.GetDateTime(reader.GetOrdinal("MonthStart")),
                DrinksIncome = GetDecimal(reader, "DrinksIncome"),
                LunchIncome = GetDecimal(reader, "LunchIncome"),
                DinnerIncome = GetDecimal(reader, "DinnerIncome")
            };
        }

        private static int GetInt(SqlDataReader reader, string columnName)
        {
            return Convert.ToInt32(reader[columnName]);
        }

        private static decimal GetDecimal(SqlDataReader reader, string columnName)
        {
            return Convert.ToDecimal(reader[columnName]);
        }

        private static string GetString(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];
            return value is DBNull ? string.Empty : Convert.ToString(value) ?? string.Empty;
        }

        // Payment bepaalt welke orders betaald zijn. Daarna rekenen we per betaalde order de omzet uit.
        // Zo hoeft er niks aangepast te worden aan de bestaande database.
        private const string PaidItemsQuery = """
            WITH paid AS (
                SELECT DISTINCT p.OrderID
                FROM [Payment] AS p
                INNER JOIN Orders AS o ON o.OrderID = p.OrderID
                WHERE o.OrderDate >= @StartDate
                  AND o.OrderDate < @EndDateExclusive
            ),
            item_parts AS (
                SELECT
                    o.OrderID,
                    o.OrderDate,
                    c.MenuCardID,
                    mc.Name AS MenuCardName,
                    ISNULL(SUM(oi.AmountOrdered), 0) AS SalesCount,
                    ISNULL(SUM(CAST(oi.AmountOrdered AS DECIMAL(18, 2)) * m.Price *
                        CASE WHEN m.IsAlcoholic = 1 THEN 1.21 ELSE 1.06 END), 0) AS TotalAmount
                FROM paid
                INNER JOIN Orders AS o ON o.OrderID = paid.OrderID
                INNER JOIN OrderItem AS oi ON oi.OrderID = paid.OrderID
                INNER JOIN MenuItems AS m ON m.MenuItemID = oi.MenuItemID
                INNER JOIN Categories AS c ON c.CategoryID = m.CategoryID
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                GROUP BY o.OrderID, o.OrderDate, c.MenuCardID, mc.Name
            )
            """;

        private const string SummaryByMenuCardQuery = PaidItemsQuery + """
            SELECT
                MenuCardID,
                MenuCardName,
                ISNULL(SUM(SalesCount), 0) AS SalesCount,
                ISNULL(SUM(TotalAmount), 0) AS TotalIncome
            FROM item_parts
            GROUP BY MenuCardID, MenuCardName
            ORDER BY MenuCardID;
            """;

        private const string TotalTipsQuery = """
            SELECT ISNULL(SUM(ISNULL(p.TotalTipAmount, 0)), 0) AS TotalTips
            FROM [Payment] AS p
            INNER JOIN Orders AS o ON o.OrderID = p.OrderID
            WHERE o.OrderDate >= @StartDate
              AND o.OrderDate < @EndDateExclusive;
            """;

        private const string MonthlyTrendQuery = PaidItemsQuery + """
            SELECT
                DATEFROMPARTS(YEAR(OrderDate), MONTH(OrderDate), 1) AS MonthStart,
                ISNULL(SUM(CASE WHEN MenuCardID = @DrinksCardId THEN TotalAmount ELSE 0 END), 0) AS DrinksIncome,
                ISNULL(SUM(CASE WHEN MenuCardID = @LunchCardId THEN TotalAmount ELSE 0 END), 0) AS LunchIncome,
                ISNULL(SUM(CASE WHEN MenuCardID = @DinnerCardId THEN TotalAmount ELSE 0 END), 0) AS DinnerIncome
            FROM item_parts
            GROUP BY DATEFROMPARTS(YEAR(OrderDate), MONTH(OrderDate), 1)
            ORDER BY MonthStart;
            """;
    }
}
