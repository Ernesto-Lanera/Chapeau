using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;
using System.Data;

namespace Chapeau.Repositories
{
    public class RoleRepository(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                                ?? throw new Exception("Database connection string is missing.");

        public List<Role> GetRoles()
        {
            var roles = new List<Role>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "SELECT RoleID, RoleName FROM Roles ORDER BY RoleName";
                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    int roleIdOrdinal = reader.GetOrdinal("RoleID");
                    int roleNameOrdinal = reader.GetOrdinal("RoleName");

                    while (reader.Read())
                    {
                        var role = new Role
                        {
                            RoleID = reader.GetInt32(roleIdOrdinal),
                            RoleName = reader.GetString(roleNameOrdinal)
                        };

                        roles.Add(role);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving roles: {ex.Message}", ex);
            }

            return roles;
        }
    }
}
