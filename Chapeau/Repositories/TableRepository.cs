using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Chapeau.Emums;

namespace Chapeau.Repositories
{
    /// <summary>
    /// Repository for table management including occupation status and order tracking.
    /// </summary>
    public class TableRepository : ITableRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TableRepository> _logger;

        public TableRepository(IConfiguration configuration, ILogger<TableRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new InvalidOperationException("Connection string 'ChapeauDatabaseSQL' not found.");
            _logger = logger;
        }

        /// <summary>
        /// Ensures the IsManuallyOccupied column exists on the Table_ table.
        /// Gracefully handles cases where the column already exists or schema is locked.
        /// </summary>
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add IsManuallyOccupied column; column may already exist or schema is locked.");
            }
        }

        /// <summary>
        /// Sets the manual occupation status of a table.
        /// </summary>
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

        /// <summary>
        /// Checks if a table has active (non-paid) orders.
        /// </summary>
        public bool HasActiveOrders(int tableId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT COUNT(1) FROM Orders
                WHERE TableID = @TableID AND OrderStatus <> @Paid";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TableID", tableId);
            command.Parameters.AddWithValue("@Paid", (int)OrderStatus.Paid);
            int count = (int)command.ExecuteScalar();
            return count > 0;
        }
    }
}
