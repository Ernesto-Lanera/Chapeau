using Chapeau.Emums;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories.Payment
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new Exception("Database connection string is missing.");
        }

        public void SavePayment(int orderId, decimal tipAmount, string? feedback)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            // De betaling en orderstatus horen bij elkaar. Daarom gebeurt dit in één transaction.
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                int tableId = GetTableIdForPayment(orderId, connection, transaction);

                SavePaymentRow(orderId, tableId, tipAmount, feedback, connection, transaction);
                MarkOrderAsPaid(orderId, tableId, connection, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static int GetTableIdForPayment(int orderId, SqlConnection connection, SqlTransaction transaction)
        {
            const string query = "SELECT TableID, OrderStatus FROM Orders WHERE OrderID = @OrderID";

            using SqlCommand command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@OrderID", orderId);

            using SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("Order not found.");
            }

            OrderStatus status = (OrderStatus)(int)reader["OrderStatus"];
            if (status != OrderStatus.Served && status != OrderStatus.Paid)
            {
                throw new InvalidOperationException("Order is not served yet.");
            }

            return (int)reader["TableID"];
        }

        private static void SavePaymentRow(
            int orderId,
            int tableId,
            decimal tipAmount,
            string? feedback,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            const string query = """
                INSERT INTO Payment (OrderID, TableID, TotalTipAmount, Feedback)
                VALUES (@OrderID, @TableID, @TipAmount, @Feedback);
                """;

            using SqlCommand command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@OrderID", orderId);
            command.Parameters.AddWithValue("@TableID", tableId);
            command.Parameters.AddWithValue("@TipAmount", tipAmount);
            command.Parameters.AddWithValue("@Feedback", (object?)feedback ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        private static void MarkOrderAsPaid(int orderId, int tableId, SqlConnection connection, SqlTransaction transaction)
        {
            const string query = """
                UPDATE Orders
                SET OrderStatus = @Paid
                WHERE OrderID = @OrderID;

                UPDATE Table_
                SET IsManuallyOccupied = 0
                WHERE TableID = @TableID;
                """;

            using SqlCommand command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@Paid", (int)OrderStatus.Paid);
            command.Parameters.AddWithValue("@OrderID", orderId);
            command.Parameters.AddWithValue("@TableID", tableId);
            command.ExecuteNonQuery();
        }
    }
}
