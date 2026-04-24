using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;

namespace Chapeau.Repositories
{
    public class MenuRepository
    {
        private readonly string _connectionString;

        public MenuRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL") 
                                ?? throw new Exception("Database connection string is missing.");
        }

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            var menuItems = new List<MenuItem>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT MenuItemID, Name, Price, Stock, IsActive, CategoryID FROM MenuItems WHERE 1=1";
                if (categoryId.HasValue)
                {
                    query += " AND CategoryID = @CategoryID";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (categoryId.HasValue)
                    {
                        command.Parameters.AddWithValue("@CategoryID", categoryId.Value);
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var menuItem = new MenuItem
                            {
                                MenuItemID = (int)reader["MenuItemID"],
                                Name = (string)reader["Name"],
                                Price = (decimal)reader["Price"],
                                Stock = (int)reader["Stock"],
                                IsActive = (bool)reader["IsActive"],
                                CategoryID = (int)reader["CategoryID"]
                            };
                            menuItems.Add(menuItem);
                        }
                    }
                }
            }

            return menuItems;
        }

        public void AddMenuItem(MenuItem item)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO MenuItems (Name, Price, Stock, IsActive, CategoryID) VALUES (@Name, @Price, @Stock, @IsActive, @CategoryID)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@Price", item.Price);
                    command.Parameters.AddWithValue("@Stock", item.Stock);
                    command.Parameters.AddWithValue("@IsActive", item.IsActive);
                    command.Parameters.AddWithValue("@CategoryID", item.CategoryID);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateMenuItem(MenuItem item)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE MenuItems SET Name = @Name, Price = @Price, Stock = @Stock, IsActive = @IsActive, CategoryID = @CategoryID WHERE MenuItemID = @MenuItemID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@Price", item.Price);
                    command.Parameters.AddWithValue("@Stock", item.Stock);
                    command.Parameters.AddWithValue("@IsActive", item.IsActive);
                    command.Parameters.AddWithValue("@CategoryID", item.CategoryID);
                    command.Parameters.AddWithValue("@MenuItemID", item.MenuItemID);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void SetMenuItemActive(int id, bool active)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE MenuItems SET IsActive = @IsActive WHERE MenuItemID = @MenuItemID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsActive", active);
                    command.Parameters.AddWithValue("@MenuItemID", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void ChangeStock(int id, int newStock)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE MenuItems SET Stock = @Stock WHERE MenuItemID = @MenuItemID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Stock", newStock);
                    command.Parameters.AddWithValue("@MenuItemID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
