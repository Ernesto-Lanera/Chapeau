using System;
using System.Collections.Generic;
using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chapeau.Repositories
{
    public class MenuRepository(IConfiguration configuration, ILogger<MenuRepository> logger)
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);

        private readonly ILogger<MenuRepository> _logger = logger;

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            var menuItems = new List<MenuItem>();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    SELECT 
                        m.MenuItemID,
                        m.Name,
                        m.RetailPrice,
                        m.Stock,
                        m.IsActive,
                        m.CategoryID,
                        c.Name AS CategoryName,
                        c.MenuCardID
                    FROM MenuItems m
                    LEFT JOIN Categories c ON c.CategoryID = m.CategoryID
                    WHERE 1 = 1";

                if (cardId.HasValue)
                {
                    query += " AND c.MenuCardID = @MenuCardID";
                }

                if (categoryId.HasValue)
                {
                    query += " AND m.CategoryID = @CategoryID";
                }

                query += " ORDER BY m.MenuItemID ASC";

                using SqlCommand command = new(query, connection);

                if (cardId.HasValue)
                {
                    command.Parameters.Add("@MenuCardID", SqlDbType.Int).Value = cardId.Value;
                }

                if (categoryId.HasValue)
                {
                    command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId.Value;
                }

                using SqlDataReader reader = command.ExecuteReader();

                int menuItemIdOrdinal = reader.GetOrdinal("MenuItemID");
                int nameOrdinal = reader.GetOrdinal("Name");
                int retailPriceOrdinal = reader.GetOrdinal("RetailPrice");
                int stockOrdinal = reader.GetOrdinal("Stock");
                int isActiveOrdinal = reader.GetOrdinal("IsActive");
                int categoryIdOrdinal = reader.GetOrdinal("CategoryID");
                int categoryNameOrdinal = reader.GetOrdinal("CategoryName");
                int menuCardIdOrdinal = reader.GetOrdinal("MenuCardID");

                while (reader.Read())
                {
                    int categoryIdValue = reader.IsDBNull(categoryIdOrdinal)
                        ? 0
                        : reader.GetInt32(categoryIdOrdinal);

                    var menuItem = new MenuItem
                    {
                        MenuItemID = reader.GetInt32(menuItemIdOrdinal),
                        Name = reader.IsDBNull(nameOrdinal) ? string.Empty : reader.GetString(nameOrdinal),
                        RetailPrice = reader.GetDecimal(retailPriceOrdinal),
                        Stock = reader.GetInt32(stockOrdinal),
                        IsActive = reader.GetBoolean(isActiveOrdinal),
                        CategoryID = categoryIdValue,
                        Category = new Category
                        {
                            CategoryID = categoryIdValue,
                            Name = reader.IsDBNull(categoryNameOrdinal) ? string.Empty : reader.GetString(categoryNameOrdinal),
                            MenuCardID = reader.IsDBNull(menuCardIdOrdinal) ? 0 : reader.GetInt32(menuCardIdOrdinal)
                        }
                    };

                    menuItems.Add(menuItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving menu items");
                throw new InvalidOperationException(ErrorMessages.RetrieveMenuItemsError, ex);
            }

            return menuItems;
        }

        public void AddMenuItem(MenuItem item)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    INSERT INTO MenuItems 
                        (Name, RetailPrice, Stock, IsActive, CategoryID) 
                    VALUES 
                        (@Name, @RetailPrice, @Stock, @IsActive, @CategoryID)";

                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(item.Name) ? DBNull.Value : item.Name;

                var retailPriceParameter = command.Parameters.Add("@RetailPrice", SqlDbType.Decimal);
                retailPriceParameter.Precision = 10;
                retailPriceParameter.Scale = 2;
                retailPriceParameter.Value = item.RetailPrice;

                command.Parameters.Add("@Stock", SqlDbType.Int).Value = item.Stock;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
                command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = item.CategoryID;

                command.ExecuteNonQuery();

                _logger.LogInformation("Menu item added: {ItemName}", item.Name);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                _logger.LogWarning("Duplicate menu item name: {ItemName}", item.Name);
                throw new InvalidOperationException(ErrorMessages.MenuItemDuplicateName, ex);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error adding menu item");
                throw new InvalidOperationException(ErrorMessages.AddMenuItemError, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding menu item");
                throw new InvalidOperationException(ErrorMessages.AddMenuItemError, ex);
            }
        }

        public void UpdateMenuItem(MenuItem item)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    UPDATE MenuItems 
                    SET 
                        Name = @Name,
                        RetailPrice = @RetailPrice,
                        Stock = @Stock,
                        IsActive = @IsActive,
                        CategoryID = @CategoryID
                    WHERE MenuItemID = @MenuItemID";

                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(item.Name) ? DBNull.Value : item.Name;

                var retailPriceParameter = command.Parameters.Add("@RetailPrice", SqlDbType.Decimal);
                retailPriceParameter.Precision = 10;
                retailPriceParameter.Scale = 2;
                retailPriceParameter.Value = item.RetailPrice;

                command.Parameters.Add("@Stock", SqlDbType.Int).Value = item.Stock;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
                command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = item.CategoryID;
                command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = item.MenuItemID;

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException(ErrorMessages.MenuItemNotFound);
                }

                _logger.LogInformation("Menu item updated: {ItemId}", item.MenuItemID);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                _logger.LogWarning("Duplicate menu item name: {ItemName}", item.Name);
                throw new InvalidOperationException(ErrorMessages.UpdateMenuItemAlreadyExists, ex);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error updating menu item");
                throw new InvalidOperationException(ErrorMessages.UpdateMenuItemError, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating menu item");
                throw new InvalidOperationException(ErrorMessages.UpdateMenuItemError, ex);
            }
        }

        public void SetMenuItemActive(int id, bool active)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    UPDATE MenuItems 
                    SET IsActive = @IsActive 
                    WHERE MenuItemID = @MenuItemID";

                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;
                command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = id;

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException(ErrorMessages.MenuItemNotFound);
                }

                _logger.LogInformation("Menu item {ItemId} active status changed to {Active}", id, active);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item activity");
                throw new InvalidOperationException(ErrorMessages.UpdateMenuItemError, ex);
            }
        }

        public void ChangeStock(int id, int newStock)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    UPDATE MenuItems 
                    SET Stock = @Stock 
                    WHERE MenuItemID = @MenuItemID";

                using SqlCommand command = new(query, connection);

                command.Parameters.Add("@Stock", SqlDbType.Int).Value = newStock;
                command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = id;

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException(ErrorMessages.MenuItemNotFound);
                }

                _logger.LogInformation("Menu item {ItemId} stock updated to {Stock}", id, newStock);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item stock");
                throw new InvalidOperationException(ErrorMessages.UpdateMenuItemError, ex);
            }
        }
    }
}