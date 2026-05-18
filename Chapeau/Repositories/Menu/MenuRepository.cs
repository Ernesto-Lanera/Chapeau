using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chapeau.Repositories.Menu
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MenuRepository> _logger;

        public MenuRepository(IConfiguration configuration, ILogger<MenuRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new Exception(ErrorMessages.ConnectionStringMissing);
            _logger = logger;
        }

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
                    ImagePath
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

        public void AddMenuItem(MenuItem menuItem)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO MenuItems
                    (Name, Price, Stock, IsActive, CategoryID, ImagePath)
                VALUES
                    (@Name, @Price, @Stock, @IsActive, @CategoryID, @ImagePath)";

            using SqlCommand command = new SqlCommand(query, connection);
            AddMenuItemParameters(command, menuItem);

            command.ExecuteNonQuery();
        }

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
                    ImagePath = @ImagePath
                WHERE MenuItemID = @MenuItemID";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MenuItemID", menuItem.MenuItemID);
            AddMenuItemParameters(command, menuItem);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Menu item not found during update: {MenuItemId}", menuItem.MenuItemID);
                throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);
            }
        }

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
                    c.CategoryID,
                    c.Name AS CategoryName,
                    c.MenuCardID
                FROM MenuItems mi
                INNER JOIN Categories c ON mi.CategoryID = c.CategoryID
                WHERE mi.IsActive = 1";

            if (cardId.HasValue)
            {
                query += " AND c.MenuCardID = @MenuCardID";
            }

            if (categoryId.HasValue)
            {
                query += " AND mi.CategoryID = @CategoryID";
            }

            query += " ORDER BY c.MenuCardID, c.Name, mi.Name";

            return query;
        }

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

        private static void AddMenuItemParameters(SqlCommand command, MenuItem menuItem)
        {
            command.Parameters.AddWithValue("@Name", menuItem.Name);
            command.Parameters.AddWithValue("@Price", menuItem.RetailPrice);
            command.Parameters.AddWithValue("@Stock", menuItem.Stock);
            command.Parameters.AddWithValue("@IsActive", menuItem.IsActive);
            command.Parameters.AddWithValue("@CategoryID", menuItem.CategoryID);

            if (string.IsNullOrWhiteSpace(menuItem.ImagePath))
            {
                command.Parameters.AddWithValue("@ImagePath", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@ImagePath", menuItem.ImagePath);
            }
        }

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