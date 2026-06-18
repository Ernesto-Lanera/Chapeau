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
            //transaction, alles gaat goed of niet, doet alle sql in 1x zodat je niet te veel connections hebt.
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {

                int tableId;
                string orderInfoQuery = "SELECT TableID, OrderStatus FROM Orders WHERE OrderID = @OrderID"; //zoekt de order, tableid gaat ook via order.
                using (SqlCommand command = new SqlCommand(orderInfoQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    using SqlDataReader reader = command.ExecuteReader();
                    if (!reader.Read())
                        throw new InvalidOperationException("Order not found.");

                    tableId = (int)reader["TableID"];
                    var status = (OrderStatus)(int)reader["OrderStatus"];
                    //order moet wel served zijn duh
                    if (status != OrderStatus.Served && status != OrderStatus.Paid)
                        throw new InvalidOperationException("Order is not served yet.");
                }

                string insertPaymentQuery = @"INSERT INTO Payment (OrderID, TableID, TotalTipAmount, Feedback)
                   VALUES (@OrderID, @TableID, @TipAmount, @Feedback)";
                using (SqlCommand command = new SqlCommand(insertPaymentQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    command.Parameters.AddWithValue("@TableID", tableId);
                    command.Parameters.AddWithValue("@TipAmount", tipAmount);
                    command.Parameters.AddWithValue("@Feedback", (object?)feedback ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
                //order betaald en tafel weer vrij
                string updateQuery = @"UPDATE Orders SET OrderStatus = @Paid WHERE OrderID = @OrderID;
                       UPDATE Table_ SET IsManuallyOccupied = 0 WHERE TableID = @TableID";

                using (SqlCommand command = new SqlCommand(updateQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@Paid", (int)OrderStatus.Paid);
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    command.Parameters.AddWithValue("@TableID", tableId);
                    command.ExecuteNonQuery();
                }

                transaction.Commit(); //commit als alles goed is.
            }
            catch
            {
                transaction.Rollback(); //rollback als er iets fout gaat met de transaction.
                throw; //throw naar controller.
            }
        }
    }
}