using System;
using System.Collections.Generic;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chapeau.Repositories
{
    public class CategoryRepository(IConfiguration configuration, ILogger<CategoryRepository> logger)
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        
        private readonly ILogger<CategoryRepository> _logger = logger;

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
                    categories = MapCategoriesFromReader(reader);

                _logger.LogInformation("Retrieved {CategoryCount} categories", categories.Count);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving categories");
                throw new InvalidOperationException(ErrorMessages.RetrieveCategoriesError, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving categories");
                throw new InvalidOperationException(ErrorMessages.RetrieveCategoriesError, ex);
            }

            return categories;
        }

        // ===== HELPER METHODS =====

        private List<Category> MapCategoriesFromReader(SqlDataReader reader)
        {
            var categories = new List<Category>();
            int idOrdinal = reader.GetOrdinal("CategoryID");
            int nameOrdinal = reader.GetOrdinal("Name");
            int menuCardIdOrdinal = reader.GetOrdinal("MenuCardID");

            while (reader.Read())
                categories.Add(MapCategoryRow(reader, idOrdinal, nameOrdinal, menuCardIdOrdinal));

            return categories;
        }

        private Category MapCategoryRow(SqlDataReader reader, int idOrdinal, int nameOrdinal, int menuCardIdOrdinal)
        {
            return new Category
            {
                CategoryID = reader.GetInt32(idOrdinal),
                Name = reader.IsDBNull(nameOrdinal) ? string.Empty : reader.GetString(nameOrdinal),
                MenuCardID = reader.GetInt32(menuCardIdOrdinal)
            };
        }
    }
}