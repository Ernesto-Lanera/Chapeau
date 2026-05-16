using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;

namespace Chapeau.Repositories.Role
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
                    SELECT PermissionName
                    FROM RolePermissions
                    WHERE RoleID = @RoleID";

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

        public RoleModel? GetRoleById(int id)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "SELECT RoleID, RoleName FROM Roles WHERE RoleID = @RoleID";
                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@RoleID", SqlDbType.Int).Value = id;
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new RoleModel
                    {
                        RoleID = reader.GetInt32(0),
                        RoleName = reader.GetString(1)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the role: {ex.Message}", ex);
            }
        }
    }
}