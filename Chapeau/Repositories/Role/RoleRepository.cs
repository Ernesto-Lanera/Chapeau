using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;

namespace Chapeau.Repositories
{
    public class RoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new Exception("Database connection string is missing.");
        }

        /// <summary>
        /// Gets all roles from the database.
        /// </summary>
        public List<EmployeeRole> GetRoles()
        {
            var roles = new List<EmployeeRole>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "SELECT RoleID, RoleName FROM Roles ORDER BY RoleName";

                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    roles.Add(new EmployeeRole
                    {
                        RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                        RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving roles: {ex.Message}", ex);
            }

            return roles;
        }

        /// <summary>
        /// Gets all permissions for a specific role.
        /// </summary>
        public List<string> GetRolePermissions(int roleId)
        {
            var permissions = new List<string>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    SELECT p.PermissionName
                    FROM RolePermissions rp
                    INNER JOIN Permissions p ON rp.PermissionID = p.PermissionID
                    WHERE rp.RoleID = @RoleID
                    ORDER BY p.PermissionName";

                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@RoleID", roleId);

                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    permissions.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving role permissions: {ex.Message}", ex);
            }

            return permissions;
        }

        /// <summary>
        /// Gets all permission names from the Permissions table.
        /// </summary>
        public List<string> GetAllPermissionNames()
        {
            var permissions = new List<string>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "SELECT PermissionName FROM Permissions ORDER BY PermissionName";

                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    permissions.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving all permission names: {ex.Message}", ex);
            }

            return permissions;
        }

        /// <summary>
        /// Replaces all permissions for a specific role.
        /// </summary>
        public void SetRolePermissions(int roleId, List<string> permissionNames)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                using SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    var permissionIds = new Dictionary<string, int>();

                    using (SqlCommand lookupCommand = new(
                        "SELECT PermissionID, PermissionName FROM Permissions", connection, transaction))
                    using (SqlDataReader reader = lookupCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissionIds[reader.GetString(1)] = reader.GetInt32(0);
                        }
                    }

                    using (SqlCommand deleteCommand = new("DELETE FROM RolePermissions WHERE RoleID = @RoleID", connection, transaction))
                    {
                        deleteCommand.Parameters.AddWithValue("@RoleID", roleId);
                        deleteCommand.ExecuteNonQuery();
                    }

                    foreach (var permissionName in permissionNames)
                    {
                        if (!permissionIds.TryGetValue(permissionName, out var permId))
                            continue;

                        using SqlCommand insertCommand = new(
                            "INSERT INTO RolePermissions (RoleID, PermissionID) VALUES (@RoleID, @PermissionID)",
                            connection, transaction);

                        insertCommand.Parameters.AddWithValue("@RoleID", roleId);
                        insertCommand.Parameters.AddWithValue("@PermissionID", permId);
                        insertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while setting role permissions: {ex.Message}", ex);
            }
        }
    }
}