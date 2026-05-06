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

                string query = "SELECT MenuItemID, Name, PurchasePrice, RetailPrice, Stock, IsActive, CategoryID FROM MenuItems WHERE 1=1";
                if (categoryId.HasValue)
                    query += " AND m.CategoryID = @CategoryID";

                using SqlCommand command = new(query, connection);
                if (categoryId.HasValue)
                    command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId.Value;

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

                string query = "INSERT INTO MenuItems (Name, PurchasePrice, RetailPrice, Stock, IsActive, CategoryID) VALUES (@Name, @PurchasePrice, @RetailPrice, @Stock, @IsActive, @CategoryID)";
                using SqlCommand command = new (query, connection);

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (object)item.Name ?? DBNull.Value;
                command.Parameters.Add("@PurchasePrice", SqlDbType.Decimal).Value = item.PurchasePrice;
                command.Parameters.Add("@RetailPrice", SqlDbType.Decimal).Value = item.RetailPrice;
                command.Parameters.Add("@Stock", SqlDbType.Int).Value = item.Stock;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
                command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = item.CategoryID;

                command.ExecuteNonQuery();

                _logger.LogInformation("Menu item added: {ItemName}", item.Name);
            }
            catch (SqlException ex) when (ex.Number == 2627)
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
                    throw new KeyNotFoundException(ErrorMessages.MenuItemNotFound);

                _logger.LogInformation("Menu item updated: {ItemId}", item.MenuItemID);
            }
            catch (SqlException ex) when (ex.Number == 2627)
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

                string query = "UPDATE MenuItems SET IsActive = @IsActive WHERE MenuItemID = @MenuItemID";
                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;
                command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = id;

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                    throw new KeyNotFoundException(ErrorMessages.MenuItemNotFound);

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

                string query = "UPDATE MenuItems SET Stock = @Stock WHERE MenuItemID = @MenuItemID";
                using SqlCommand command = new(query, connection);
                command.Parameters.Add("@Stock", SqlDbType.Int).Value = newStock;
                command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = id;

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                    throw new KeyNotFoundException(ErrorMessages.MenuItemNotFound);

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

        // ===== HELPER METHODS =====

        private List<MenuItem> MapMenuItemsFromReader(SqlDataReader reader)
        {
            var menuItems = new List<MenuItem>();
            int menuItemIdOrdinal = reader.GetOrdinal("MenuItemID");
            int nameOrdinal = reader.GetOrdinal("Name");
            int priceOrdinal = reader.GetOrdinal("Price");
            int stockOrdinal = reader.GetOrdinal("Stock");
            int isActiveOrdinal = reader.GetOrdinal("IsActive");
            int categoryIdOrdinal = reader.GetOrdinal("CategoryID");
            int categoryNameOrdinal = reader.GetOrdinal("CategoryName");
            int menuCardIdOrdinal = reader.GetOrdinal("MenuCardID");

            while (reader.Read())
                menuItems.Add(MapMenuItemRow(reader, menuItemIdOrdinal, nameOrdinal, priceOrdinal, 
                    stockOrdinal, isActiveOrdinal, categoryIdOrdinal, categoryNameOrdinal, menuCardIdOrdinal));

            return menuItems;
        }

        private MenuItem MapMenuItemRow(SqlDataReader reader, int menuItemIdOrdinal, int nameOrdinal, 
            int priceOrdinal, int stockOrdinal, int isActiveOrdinal, int categoryIdOrdinal, 
            int categoryNameOrdinal, int menuCardIdOrdinal)
        {
            int catId = reader.GetInt32(categoryIdOrdinal);
            string catName = reader.IsDBNull(categoryNameOrdinal) ? string.Empty : reader.GetString(categoryNameOrdinal);
            int menuCardId = reader.IsDBNull(menuCardIdOrdinal) ? 0 : reader.GetInt32(menuCardIdOrdinal);

            return new MenuItem
            {
                MenuItemID = reader.GetInt32(menuItemIdOrdinal),
                Name = reader.IsDBNull(nameOrdinal) ? string.Empty : reader.GetString(nameOrdinal),
                Price = reader.GetDecimal(priceOrdinal),
                Stock = reader.GetInt32(stockOrdinal),
                IsActive = reader.GetBoolean(isActiveOrdinal),
                CategoryID = catId,
                Category = new Category
                {
                    CategoryID = catId,
                    Name = catName,
                    MenuCardID = menuCardId
                }
            };
        }

        private void AddMenuItemParameters(SqlCommand command, MenuItem item)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, DatabaseConstraints.MenuItemNameMaxLength)
                .Value = (object?)item.Name ?? DBNull.Value;
            command.Parameters.Add("@Price", SqlDbType.Decimal).Value = item.Price;
            command.Parameters.Add("@Stock", SqlDbType.Int).Value = item.Stock;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = item.CategoryID;
        }
    }
}
