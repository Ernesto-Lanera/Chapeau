using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories
{
    public class RoleRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(IConfiguration configuration, ILogger<RoleRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gets all permissions for a specific role.
        /// </summary>
        public List<string> GetRolePermissions(int roleId)
        {
            var permissions = new List<string>();

            try
            {
                string connectionString = _configuration.GetConnectionString("ChapeauDatabaseSQL")
                    ?? throw new InvalidOperationException("Connection string is missing");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT p.PermissionName
                        FROM RolePermissions rp
                        INNER JOIN Permissions p ON rp.PermissionID = p.PermissionID
                        WHERE rp.RoleID = @RoleID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", roleId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                permissions.Add((string)reader["PermissionName"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for role {RoleId}", roleId);
            }

            return permissions;
        }

        /// <summary>
        /// Gets the role name for a given role ID.
        /// </summary>
        public string GetRoleName(int roleId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("ChapeauDatabaseSQL")
                    ?? throw new InvalidOperationException("Connection string is missing");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT RoleName FROM Roles WHERE RoleID = @RoleID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", roleId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (string)reader["RoleName"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role name for role {RoleId}", roleId);
            }

            return string.Empty;
        }
    }
}
