using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories.Menu
{
    public class MenuRepository(IConfiguration configuration, ILogger<MenuRepository> logger) : IMenuRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        private readonly ILogger<MenuRepository> _logger = logger;

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(BuildMenuItemsQuery(cardId, categoryId), connection);
            AddFilterParameters(command, cardId, categoryId);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            var menuItems = new List<MenuItem>();
            while (reader.Read())
            {
                menuItems.Add(MapMenuItem(reader));
            }

            return menuItems;
        }

        public MenuItem? GetMenuItemById(int menuItemId)
        {
            const string query = """
                SELECT mi.MenuItemID, mi.Name, mi.Price, mi.Stock, mi.IsActive, mi.CategoryID,
                       mi.ImagePath, mi.IsAlcoholic, c.Name AS CategoryName, c.MenuCardID,
                       mc.Name AS MenuCardName
                FROM MenuItems AS mi
                INNER JOIN Categories AS c ON c.CategoryID = mi.CategoryID
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                WHERE mi.MenuItemID = @MenuItemID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = menuItemId;
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapMenuItem(reader) : null;
        }

        public bool NameExists(string name, int? excludedMenuItemId = null)
        {
            const string query = """
                SELECT COUNT(1)
                FROM MenuItems
                WHERE LOWER(Name) = LOWER(@Name)
                  AND (@ExcludedMenuItemID IS NULL OR MenuItemID <> @ExcludedMenuItemID);
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name.Trim();
            command.Parameters.Add("@ExcludedMenuItemID", SqlDbType.Int).Value =
                excludedMenuItemId.HasValue ? excludedMenuItemId.Value : DBNull.Value;
            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            const string query = """
                INSERT INTO MenuItems (Name, Price, Stock, IsActive, CategoryID, ImagePath, IsAlcoholic)
                OUTPUT INSERTED.MenuItemID
                VALUES (@Name, @Price, @Stock, @IsActive, @CategoryID, @ImagePath, @IsAlcoholic);
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            AddEditableParameters(command, menuItem);
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = menuItem.IsActive;
            connection.Open();

            menuItem.MenuItemID = Convert.ToInt32(command.ExecuteScalar());
            _logger.LogInformation("Menu-item toegevoegd: {MenuItemId} - {Name}.", menuItem.MenuItemID, menuItem.Name);
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            const string query = """
                UPDATE MenuItems
                SET Name = @Name, Price = @Price, Stock = @Stock, CategoryID = @CategoryID,
                    ImagePath = @ImagePath, IsAlcoholic = @IsAlcoholic
                WHERE MenuItemID = @MenuItemID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = menuItem.MenuItemID;
            AddEditableParameters(command, menuItem);
            connection.Open();
            EnsureUpdated(command.ExecuteNonQuery(), menuItem.MenuItemID);
        }

        public void SetMenuItemActive(int menuItemId, bool active)
        {
            const string query = "UPDATE MenuItems SET IsActive = @IsActive WHERE MenuItemID = @MenuItemID;";

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = menuItemId;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;
            connection.Open();
            EnsureUpdated(command.ExecuteNonQuery(), menuItemId);
        }

        public void ChangeStock(int menuItemId, int newStock)
        {
            const string query = "UPDATE MenuItems SET Stock = @Stock WHERE MenuItemID = @MenuItemID;";

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = menuItemId;
            command.Parameters.Add("@Stock", SqlDbType.Int).Value = newStock;
            connection.Open();
            EnsureUpdated(command.ExecuteNonQuery(), menuItemId);
        }

        private static string BuildMenuItemsQuery(int? cardId, int? categoryId)
        {
            string query = """
                SELECT mi.MenuItemID, mi.Name, mi.Price, mi.Stock, mi.IsActive, mi.CategoryID,
                       mi.ImagePath, mi.IsAlcoholic, c.Name AS CategoryName, c.MenuCardID,
                       mc.Name AS MenuCardName
                FROM MenuItems AS mi
                INNER JOIN Categories AS c ON c.CategoryID = mi.CategoryID
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                """;

            var filters = new List<string>();
            if (cardId.HasValue) filters.Add("c.MenuCardID = @MenuCardID");
            if (categoryId.HasValue) filters.Add("mi.CategoryID = @CategoryID");
            if (filters.Count > 0) query += " WHERE " + string.Join(" AND ", filters);

            return query + " ORDER BY c.MenuCardID, c.Name, mi.Name;";
        }

        private static void AddFilterParameters(SqlCommand command, int? cardId, int? categoryId)
        {
            if (cardId.HasValue) command.Parameters.Add("@MenuCardID", SqlDbType.Int).Value = cardId.Value;
            if (categoryId.HasValue) command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId.Value;
        }

        private static void AddEditableParameters(SqlCommand command, MenuItem item)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.Name.Trim();
            SqlParameter priceParameter = command.Parameters.Add("@Price", SqlDbType.Decimal);
            priceParameter.Precision = 10;
            priceParameter.Scale = 2;
            priceParameter.Value = item.RetailPrice;
            command.Parameters.Add("@Stock", SqlDbType.Int).Value = item.Stock;
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = item.CategoryID;
            command.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 500).Value =
                string.IsNullOrWhiteSpace(item.ImagePath) ? DBNull.Value : item.ImagePath;
            command.Parameters.Add("@IsAlcoholic", SqlDbType.Bit).Value = item.IsAlcoholic;
        }

        private void EnsureUpdated(int rowsAffected, int menuItemId)
        {
            if (rowsAffected > 0) return;
            _logger.LogWarning("Menu-item niet gevonden: {MenuItemId}.", menuItemId);
            throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);
        }

        private static MenuItem MapMenuItem(SqlDataReader reader)
        {
            int menuCardId = reader.GetInt32(reader.GetOrdinal("MenuCardID"));
            var menuCard = new MenuCard
            {
                MenuCardID = menuCardId,
                Name = reader.GetString(reader.GetOrdinal("MenuCardName"))
            };

            return new MenuItem
            {
                MenuItemID = reader.GetInt32(reader.GetOrdinal("MenuItemID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                RetailPrice = reader.GetDecimal(reader.GetOrdinal("Price")),
                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("ImagePath")),
                IsAlcoholic = !reader.IsDBNull(reader.GetOrdinal("IsAlcoholic"))
                    && reader.GetBoolean(reader.GetOrdinal("IsAlcoholic")),
                Category = new Models.Category
                {
                    CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                    Name = reader.GetString(reader.GetOrdinal("CategoryName")),
                    MenuCardID = menuCardId,
                    MenuCard = menuCard
                }
            };
        }
    }
}
