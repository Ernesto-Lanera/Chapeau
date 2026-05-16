using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;
using System.Data;
using RoleModel = Chapeau.Models.Role;

namespace Chapeau.Repositories.Role
{
    public class RoleRepository(IConfiguration configuration) : IRoleRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                                ?? throw new Exception("Database connection string is missing.");

        public List<RoleModel> GetRoles()
        {
            var roles = new List<RoleModel>();

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
                        var role = new RoleModel
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
