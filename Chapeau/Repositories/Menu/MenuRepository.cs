using Chapeau.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Chapeau.Repositories.Menu
{
    public class MenuRepository : IMenuRepository
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

            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT 
                    mi.MenuItemID,
                    mi.Name,
                    mi.Price,
                    mi.Stock,
                    mi.IsActive,
                    mi.CategoryID,
                    mi.ImagePath
                FROM MenuItems mi
                INNER JOIN Categories c ON mi.CategoryID = c.CategoryID
                WHERE 1 = 1";

            if (cardId.HasValue)
            {
                query += " AND c.MenuCardID = @MenuCardID";
            }

            if (categoryId.HasValue)
            {
                query += " AND mi.CategoryID = @CategoryID";
            }

            query += @"
                ORDER BY 
                    c.MenuCardID,
                    c.Name,
                    mi.Name";

            using SqlCommand command = new SqlCommand(query, connection);

            if (cardId.HasValue)
            {
                command.Parameters.AddWithValue("@MenuCardID", cardId.Value);
            }

            if (categoryId.HasValue)
            {
                command.Parameters.AddWithValue("@CategoryID", categoryId.Value);
            }

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
            command.Parameters.AddWithValue("@Name", menuItem.Name);
            command.Parameters.AddWithValue("@Price", menuItem.RetailPrice);
            command.Parameters.AddWithValue("@Stock", menuItem.Stock);
            command.Parameters.AddWithValue("@CategoryID", menuItem.CategoryID);

            if (string.IsNullOrWhiteSpace(menuItem.ImagePath))
            {
                command.Parameters.AddWithValue("@ImagePath", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@ImagePath", menuItem.ImagePath);
            }

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new Exception("Menu item niet gevonden.");
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
                throw new Exception("Menu item niet gevonden.");
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
                throw new Exception("Menu item niet gevonden.");
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
                CategoryID = Convert.ToInt32(reader["CategoryID"])
            };

            if (reader["ImagePath"] != DBNull.Value)
            {
                menuItem.ImagePath = reader["ImagePath"].ToString();
            }

            return menuItem;
        }
    }
}