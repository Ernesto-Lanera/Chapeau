using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;
using CategoryModel = Chapeau.Models.Category;

namespace Chapeau.Repositories.Menu
{
    public class MenuRepository(IConfiguration configuration, ILogger<MenuRepository> logger) : IMenuRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        private readonly ILogger<MenuRepository> _logger = logger;

        public List<MenuItem> GetMenuItems(int? cardId, int? categoryId)
        {
            string query = BuildGetMenuItemsQuery(cardId, categoryId);

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            AddMenuItemsQueryParameters(command, cardId, categoryId);

            connection.Open();

            var menuItems = new List<MenuItem>();
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                menuItems.Add(ReadMenuItem(reader));
            }

            return menuItems;
        }

        public MenuItem? GetMenuItemById(int menuItemId)
        {
            const string query = """
                SELECT
                    mi.MenuItemID,
                    mi.Name,
                    mi.Price,
                    mi.Stock,
                    mi.IsActive,
                    mi.CategoryID,
                    mi.ImagePath,
                    mi.IsAlcoholic,
                    c.Name AS CategoryName,
                    c.MenuCardID
                FROM MenuItems AS mi
                INNER JOIN Categories AS c ON c.CategoryID = mi.CategoryID
                WHERE mi.MenuItemID = @MenuItemID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = menuItemId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? ReadMenuItem(reader) : null;
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            const string query = """
                INSERT INTO MenuItems
                    (Name, Price, Stock, IsActive, CategoryID, ImagePath, IsAlcoholic)
                VALUES
                    (@Name, @Price, @Stock, @IsActive, @CategoryID, @ImagePath, @IsAlcoholic);
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            AddEditableMenuItemParameters(command, menuItem);
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = menuItem.IsActive;

            connection.Open();
            command.ExecuteNonQuery();

            _logger.LogInformation("Menu-item toegevoegd: {MenuItemName}.", menuItem.Name);
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            const string query = """
                UPDATE MenuItems
                SET Name = @Name,
                    Price = @Price,
                    Stock = @Stock,
                    CategoryID = @CategoryID,
                    ImagePath = @ImagePath,
                    IsAlcoholic = @IsAlcoholic
                WHERE MenuItemID = @MenuItemID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = menuItem.MenuItemID;
            AddEditableMenuItemParameters(command, menuItem);

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Menu-item niet gevonden tijdens wijzigen: {MenuItemId}.", menuItem.MenuItemID);
                throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);
            }

            _logger.LogInformation("Menu-item bijgewerkt: {MenuItemId} - {MenuItemName}.", menuItem.MenuItemID, menuItem.Name);
        }

        public void SetMenuItemActive(int menuItemId, bool active)
        {
            const string query = """
                UPDATE MenuItems
                SET IsActive = @IsActive
                WHERE MenuItemID = @MenuItemID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = menuItemId;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = active;

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Menu-item niet gevonden tijdens statuswijziging: {MenuItemId}.", menuItemId);
                throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);
            }
        }

        public void ChangeStock(int menuItemId, int stock)
        {
            const string query = """
                UPDATE MenuItems
                SET Stock = @Stock
                WHERE MenuItemID = @MenuItemID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@MenuItemID", SqlDbType.Int).Value = menuItemId;
            command.Parameters.Add("@Stock", SqlDbType.Int).Value = stock;

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Menu-item niet gevonden tijdens voorraadwijziging: {MenuItemId}.", menuItemId);
                throw new InvalidOperationException(ErrorMessages.MenuItemNotFound);
            }
        }

        private static string BuildGetMenuItemsQuery(int? cardId, int? categoryId)
        {
            string query = """
                SELECT
                    mi.MenuItemID,
                    mi.Name,
                    mi.Price,
                    mi.Stock,
                    mi.IsActive,
                    mi.CategoryID,
                    mi.ImagePath,
                    mi.IsAlcoholic,
                    c.Name AS CategoryName,
                    c.MenuCardID
                FROM MenuItems AS mi
                INNER JOIN Categories AS c ON c.CategoryID = mi.CategoryID
                """;

            var filters = new List<string>();

            if (cardId.HasValue)
            {
                filters.Add("c.MenuCardID = @MenuCardID");
            }

            if (categoryId.HasValue)
            {
                filters.Add("mi.CategoryID = @CategoryID");
            }

            if (filters.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filters);
            }

            return query + " ORDER BY c.MenuCardID, c.Name, mi.Name;";
        }

        private static void AddMenuItemsQueryParameters(SqlCommand command, int? cardId, int? categoryId)
        {
            if (cardId.HasValue)
            {
                command.Parameters.Add("@MenuCardID", SqlDbType.Int).Value = cardId.Value;
            }

            if (categoryId.HasValue)
            {
                command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId.Value;
            }
        }

        private static void AddEditableMenuItemParameters(SqlCommand command, MenuItem menuItem)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = menuItem.Name.Trim();

            SqlParameter price = command.Parameters.Add("@Price", SqlDbType.Decimal);
            price.Precision = 10;
            price.Scale = 2;
            price.Value = menuItem.RetailPrice;

            command.Parameters.Add("@Stock", SqlDbType.Int).Value = menuItem.Stock;
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = menuItem.CategoryID;
            command.Parameters.Add("@IsAlcoholic", SqlDbType.Bit).Value = menuItem.IsAlcoholic;
            command.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 500).Value =
                string.IsNullOrWhiteSpace(menuItem.ImagePath) ? DBNull.Value : menuItem.ImagePath;
        }

        private static MenuItem ReadMenuItem(SqlDataReader reader)
        {
            return new MenuItem
            {
                MenuItemID = Convert.ToInt32(reader["MenuItemID"]),
                Name = Convert.ToString(reader["Name"]) ?? string.Empty,
                RetailPrice = Convert.ToDecimal(reader["Price"]),
                Stock = Convert.ToInt32(reader["Stock"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                ImagePath = reader["ImagePath"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(reader["ImagePath"]) ?? string.Empty,
                IsAlcoholic = reader["IsAlcoholic"] != DBNull.Value && Convert.ToBoolean(reader["IsAlcoholic"]),
                Category = new CategoryModel
                {
                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                    Name = Convert.ToString(reader["CategoryName"]) ?? string.Empty,
                    MenuCardID = Convert.ToInt32(reader["MenuCardID"])
                }
            };
        }
    }
}
