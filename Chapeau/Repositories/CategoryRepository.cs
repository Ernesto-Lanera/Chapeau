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
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT CategoryID, Name, MenuCardID FROM Categories";
                
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var category = new Category
                        {
                            CategoryID = (int)reader["CategoryID"],
                            Name = reader["Name"].ToString(),
                            MenuCardID = (int)reader["MenuCardID"]
                        };
                        categories.Add(category);
                    }
                }
            }
            return categories;
        }
    }
}