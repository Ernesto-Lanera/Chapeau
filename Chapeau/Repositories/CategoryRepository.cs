using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;

namespace Chapeau.Repositories
{
    public class CategoryRepository
    {
        private readonly string _connectionString;

        public CategoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                                ?? throw new Exception("Database connection string is missing.");

        }

        public List<Category> GetCategories()
        {
            var categories = new List<Category>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "SELECT CategoryID, Name, MenuCardID FROM Categories";
                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    int idOrdinal = reader.GetOrdinal("CategoryID");
                    int nameOrdinal = reader.GetOrdinal("Name");
                    int menuCardIdOrdinal = reader.GetOrdinal("MenuCardID");

                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            CategoryID = reader.GetInt32(idOrdinal),
                            Name = !reader.IsDBNull(nameOrdinal) ? reader.GetString(nameOrdinal) : null,
                            MenuCardID = reader.GetInt32(menuCardIdOrdinal)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving categories: {ex.Message}", ex);
            }

            return categories;
        }
    }
}