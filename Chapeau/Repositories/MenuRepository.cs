using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;

using System.Data;

namespace Chapeau.Repositories
{
    public class MenuRepository(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                                ?? throw new Exception("Database connection string is missing.");

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            var menuItems = new List<MenuItem>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "SELECT MenuItemID, Name, PurchasePrice, RetailPrice, Stock, IsActive, CategoryID FROM MenuItems WHERE 1=1";
                if (categoryId.HasValue)
                {
                    query += " AND CategoryID = @CategoryID";
                }

                using SqlCommand command = new(query, connection);
                if (categoryId.HasValue)
                {
                    command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId.Value;
                }

                using SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    int menuItemIdOrdinal = reader.GetOrdinal("MenuItemID");
                    int nameOrdinal = reader.GetOrdinal("Name");
                    int purchasePriceOrdinal = reader.GetOrdinal("PurchasePrice");
                    int retailPriceOrdinal = reader.GetOrdinal("RetailPrice");
                    int stockOrdinal = reader.GetOrdinal("Stock");
                    int isActiveOrdinal = reader.GetOrdinal("IsActive");
                    int categoryIdOrdinal = reader.GetOrdinal("CategoryID");

                    while (reader.Read())
                    {
                        var menuItem = new MenuItem
                        {
                            MenuItemID = reader.GetInt32(menuItemIdOrdinal),
                            Name = !reader.IsDBNull(nameOrdinal) ? reader.GetString(nameOrdinal) : string.Empty,
                            PurchasePrice = reader.GetDecimal(purchasePriceOrdinal),
                            RetailPrice = reader.GetDecimal(retailPriceOrdinal),
                            Stock = reader.GetInt32(stockOrdinal),
                            IsActive = reader.GetBoolean(isActiveOrdinal),
                            CategoryID = reader.GetInt32(categoryIdOrdinal)
                        };

                        menuItems.Add(menuItem);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving menu items: {ex.Message}", ex);
            }

            return menuItems;
        }

        public void AddMenuItem(MenuItem item)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "INSERT INTO MenuItems (Name, PurchasePrice, RetailPrice, Stock, IsActive, CategoryID) VALUES (@Name, @PurchasePrice, @RetailPrice, @Stock, @IsActive, @CategoryID)";
                using SqlCommand command = new (query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)item.Name ?? DBNull.Value;
                command.Parameters.Add("@PurchasePrice", SqlDbType.Decimal).Value = item.PurchasePrice;
                command.Parameters.Add("@RetailPrice", SqlDbType.Decimal).Value = item.RetailPrice;
                command.Parameters.Add("@Stock", SqlDbType.Int).Value = item.Stock;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
                command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = item.CategoryID;

                command.ExecuteNonQuery();
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                throw new InvalidOperationException("A menu item with this name already exists.");
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding the menu item: {ex.Message}", ex);
            }
        }

        public void UpdateMenuItem(MenuItem item)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "UPDATE MenuItems SET Name = @Name, PurchasePrice = @PurchasePrice, RetailPrice = @RetailPrice, Stock = @Stock, IsActive = @IsActive, CategoryID = @CategoryID WHERE MenuItemID = @MenuItemID";
                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)item.Name ?? DBNull.Value;
                command.Parameters.Add("@PurchasePrice", SqlDbType.Decimal).Value = item.PurchasePrice;
                command.Parameters.Add("@RetailPrice", SqlDbType.Decimal).Value = item.RetailPrice;
                command.Parameters.Add("@Stock", SqlDbType.Int).Value = item.Stock;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
                command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = item.CategoryID;
                command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = item.MenuItemID;

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception("Update failed: Menu item not found.");
                }
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                throw new InvalidOperationException("Another menu item with this name already exists.");
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the menu item: {ex.Message}", ex);
            }
        }

        public void SetMenuItemActive(int id, bool active)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "UPDATE MenuItems SET IsActive = @IsActive WHERE MenuItemID = @MenuItemID";
                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;
                command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = id;

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception("Update failed: Menu item not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while altering menu item activity: {ex.Message}", ex);
            }
        }

        public void ChangeStock(int id, int newStock)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = "UPDATE MenuItems SET Stock = @Stock WHERE MenuItemID = @MenuItemID";
                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Stock", SqlDbType.Int).Value = newStock;
                command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = id;

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception("Update failed: Menu item not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the stock: {ex.Message}", ex);
            }
        }
    }
}
