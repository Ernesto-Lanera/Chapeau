using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chapeau.Repositories.Menu
{
    public class MenuRepository(IConfiguration configuration, ILogger<MenuRepository> logger) : IMenuRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        private readonly ILogger<MenuRepository> _logger = logger;

        // Haalt menu-items op met optionele filter
        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            var menuItems = new List<MenuItem>();

            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = BuildGetMenuItemsQuery(cardId, categoryId);

            using SqlCommand command = new SqlCommand(query, connection);
            AddMenuItemsQueryParameters(command, cardId, categoryId);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                menuItems.Add(ReadMenuItem(reader));
            }

            return menuItems;
        }

        // Haalt specifiek menu-item op via ID
        public MenuItem? GetMenuItemById(int menuItemId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT 
                    MenuItemID,
                    Name,
                    Price,
                    Stock,
                    IsActive,
                    CategoryID,
                    ImagePath,
                    IsAlcoholic
                FROM MenuItems
                WHERE MenuItemID = @MenuItemID";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MenuItemID", menuItemId);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadMenuItem(reader);
            }

            return null;
        }

        // Voegt nieuw menu-item toe
        public void AddMenuItem(MenuItem menuItem)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO MenuItems
                    (Name, Price, Stock, IsActive, CategoryID, ImagePath, IsAlcoholic)
                VALUES
                    (@Name, @Price, @Stock, @IsActive, @CategoryID, @ImagePath, @IsAlcoholic)";

            using SqlCommand command = new SqlCommand(query, connection);
            AddMenuItemParameters(command, menuItem);

            command.ExecuteNonQuery();
            _logger.LogInformation("Menu item created: {MenuItemName} (Price: {Price})", menuItem.Name, menuItem.RetailPrice);
        }

        // Werkt menu-item bij
        public void UpdateMenuItem(MenuItem menuItem)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE MenuItems
                SET 
                    Name = @Name,
                    Price = @Price,
                    Stock = @Stock,
                    CategoryID = @CategoryID,
                    ImagePath = @ImagePath,
                    IsAlcoholic = @IsAlcoholic
                WHERE MenuItemID = @MenuItemID";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MenuItemID", menuItem.MenuItemID);
            AddMenuItemParameters(command, menuItem);

            command.ExecuteNonQuery();
            _logger.LogInformation("Menu item updated: {MenuItemID} - {MenuItemName}", menuItem.MenuItemID, menuItem.Name);
        }

        // Zet menu-item actief of inactief
        public void SetMenuItemActive(int menuItemId, bool active)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE MenuItems
                SET IsActive = @IsActive
                WHERE MenuItemID = @MenuItemID";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MenuItemID", menuItemId);
            command.Parameters.AddWithValue("@IsActive", active);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Menu item not found during status update: {MenuItemId}", menuItemId);
                throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);
            }
        }

        // Werkt voorraad bij
        public void ChangeStock(int menuItemId, int stock)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE MenuItems
                SET Stock = @Stock
                WHERE MenuItemID = @MenuItemID";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MenuItemID", menuItemId);
            command.Parameters.AddWithValue("@Stock", stock);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Menu item not found during stock change: {MenuItemId}", menuItemId);
                throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);
            }
        }

        // Helper: bouwt query voor ophalen menu-items
        private static string BuildGetMenuItemsQuery(int? cardId, int? categoryId)
        {
            string query = @"
                SELECT 
                    mi.MenuItemID,
                    mi.Name,
                    mi.Price,
                    mi.Stock,
                    mi.IsActive,
                    mi.CategoryID,
                    mi.ImagePath,
                    mi.IsAlcoholic,
                    c.CategoryID,
                    c.Name AS CategoryName,
                    c.MenuCardID
                FROM MenuItems mi
                INNER JOIN Categories c ON mi.CategoryID = c.CategoryID";

            // WHERE clause gedeactiveerd - toon alle items, ook inactieve
            var conditions = new List<string>();

            if (cardId.HasValue)
            {
                conditions.Add("c.MenuCardID = @MenuCardID");
            }

            if (categoryId.HasValue)
            {
                conditions.Add("mi.CategoryID = @CategoryID");
            }

            if (conditions.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", conditions);
            }

            query += " ORDER BY c.MenuCardID, c.Name, mi.Name";

            return query;
        }

        // Helper: voegt query parameters toe
        private static void AddMenuItemsQueryParameters(SqlCommand command, int? cardId, int? categoryId)
        {
            if (cardId.HasValue)
            {
                command.Parameters.AddWithValue("@MenuCardID", cardId.Value);
            }

            if (categoryId.HasValue)
            {
                command.Parameters.AddWithValue("@CategoryID", categoryId.Value);
            }
        }

        // Helper: voegt menu-item parameters toe aan command
        private static void AddMenuItemParameters(SqlCommand command, MenuItem menuItem)
        {
            command.Parameters.AddWithValue("@Name", menuItem.Name);
            command.Parameters.AddWithValue("@Price", menuItem.RetailPrice);
            command.Parameters.AddWithValue("@Stock", menuItem.Stock);
            command.Parameters.AddWithValue("@IsActive", menuItem.IsActive);
            command.Parameters.AddWithValue("@CategoryID", menuItem.CategoryID);
            command.Parameters.AddWithValue("@IsAlcoholic", menuItem.IsAlcoholic);

            if (string.IsNullOrWhiteSpace(menuItem.ImagePath))
            {
                command.Parameters.AddWithValue("@ImagePath", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@ImagePath", menuItem.ImagePath);
            }
        }

        // Helper: leest menu-item van database
        private MenuItem ReadMenuItem(SqlDataReader reader)
        {
            var menuItem = new MenuItem
            {
                MenuItemID = Convert.ToInt32(reader["MenuItemID"]),
                Name = reader["Name"].ToString() ?? "",
                RetailPrice = Convert.ToDecimal(reader["Price"]),
                Stock = Convert.ToInt32(reader["Stock"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                IsAlcoholic = reader["IsAlcoholic"] != DBNull.Value && Convert.ToBoolean(reader["IsAlcoholic"]),
                Category = new Models.Category
                {
                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                    Name = reader["CategoryName"].ToString() ?? "",
                    MenuCardID = Convert.ToInt32(reader["MenuCardID"])
                }
            };

            if (reader["ImagePath"] != DBNull.Value)
            {
                menuItem.ImagePath = reader["ImagePath"].ToString() ?? "";
            }

            return menuItem;
        }
    }
}