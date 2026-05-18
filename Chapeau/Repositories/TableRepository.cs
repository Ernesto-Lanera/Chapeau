using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Chapeau.Repositories
{
    public class TableRepository
    {
        private readonly string _connectionString;

        public TableRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new Exception("Database connection string is missing.");
        }

        public void EnsureColumnExists()
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();

                string checkQuery = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Table_' AND COLUMN_NAME = 'IsManuallyOccupied'";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                {
                    int exists = (int)checkCmd.ExecuteScalar();
                    if (exists == 0)
                    {
                        string alterQuery = "ALTER TABLE Table_ ADD IsManuallyOccupied BIT NOT NULL DEFAULT 0";
                        using SqlCommand alterCmd = new SqlCommand(alterQuery, connection);
                        alterCmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Column may already exist or schema is locked; continue gracefully
            }
        }

        public void SetOccupied(int tableId, bool occupied)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "UPDATE Table_ SET IsManuallyOccupied = @Occupied WHERE TableID = @TableID";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Occupied", occupied);
            command.Parameters.AddWithValue("@TableID", tableId);
            command.ExecuteNonQuery();
        }

        public bool HasActiveOrders(int tableId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT COUNT(1) FROM Orders
                WHERE TableID = @TableID AND OrderStatus <> @Paid";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TableID", tableId);
            command.Parameters.AddWithValue("@Paid", 4);
            int count = (int)command.ExecuteScalar();
            return count > 0;
        }
    }
}
