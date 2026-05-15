using System;
using System.Collections.Generic;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CategoryModel = Chapeau.Models.Category;

namespace Chapeau.Repositories.Category
{
    public class CategoryRepository(IConfiguration configuration, ILogger<CategoryRepository> logger) : ICategoryRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        
        private readonly ILogger<CategoryRepository> _logger = logger;

        public List<CategoryModel> GetCategories()
        {
            var categories = new List<CategoryModel>();

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

        public void AddCategory(CategoryModel category)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    INSERT INTO Categories (Name, MenuCardID) 
                    VALUES (@Name, @MenuCardID)";

                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 100).Value = 
                    string.IsNullOrWhiteSpace(category.Name) ? DBNull.Value : category.Name;
                command.Parameters.Add("@MenuCardID", System.Data.SqlDbType.Int).Value = category.MenuCardID;

                command.ExecuteNonQuery();

                _logger.LogInformation("Category added: {CategoryName}", category.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category");
                throw new InvalidOperationException(ErrorMessages.AddCategoryError, ex);
            }
        }

        public void UpdateCategory(CategoryModel category)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    UPDATE Categories 
                    SET Name = @Name, MenuCardID = @MenuCardID 
                    WHERE CategoryID = @CategoryID";

                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 100).Value = 
                    string.IsNullOrWhiteSpace(category.Name) ? DBNull.Value : category.Name;
                command.Parameters.Add("@MenuCardID", System.Data.SqlDbType.Int).Value = category.MenuCardID;
                command.Parameters.Add("@CategoryID", System.Data.SqlDbType.Int).Value = category.CategoryID;

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException(ErrorMessages.CategoryNotFound);
                }

                _logger.LogInformation("Category updated: {CategoryId}", category.CategoryID);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                throw new InvalidOperationException(ErrorMessages.UpdateCategoryError, ex);
            }
        }

        private List<CategoryModel> MapCategoriesFromReader(SqlDataReader reader)
        {
            var categories = new List<CategoryModel>();
            int idOrdinal = reader.GetOrdinal("CategoryID");
            int nameOrdinal = reader.GetOrdinal("Name");
            int menuCardIdOrdinal = reader.GetOrdinal("MenuCardID");

            while (reader.Read())
                categories.Add(MapCategoryRow(reader, idOrdinal, nameOrdinal, menuCardIdOrdinal));

            return categories;
        }

        private CategoryModel MapCategoryRow(SqlDataReader reader, int idOrdinal, int nameOrdinal, int menuCardIdOrdinal)
        {
            return new CategoryModel
            {
                CategoryID = reader.GetInt32(idOrdinal),
                Name = reader.IsDBNull(nameOrdinal) ? string.Empty : reader.GetString(nameOrdinal),
                MenuCardID = reader.GetInt32(menuCardIdOrdinal)
            };
        }
    }
}