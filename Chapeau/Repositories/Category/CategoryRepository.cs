using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(IConfiguration configuration, ILogger<CategoryRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
                ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
            _logger = logger;
        }

        public List<Category> GetCategories()
        {
            string query = @"
                SELECT c.CategoryID, c.Name, c.MenuCardID, mc.Name AS MenuCardName
                FROM Categories AS c
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                ORDER BY c.MenuCardID, c.Name;";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            List<Category> categories = new List<Category>();
            while (reader.Read())
            {
                categories.Add(MapCategory(reader));
            }

            return categories;
        }

        public List<Category> GetCategoriesByCard(int menuCardId)
        {
            string query = @"
                SELECT c.CategoryID, c.Name, c.MenuCardID, mc.Name AS MenuCardName
                FROM Categories AS c
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                WHERE c.MenuCardID = @MenuCardID
                ORDER BY c.Name;";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@MenuCardID", SqlDbType.Int).Value = menuCardId;
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            List<Category> categories = new List<Category>();
            while (reader.Read())
            {
                categories.Add(MapCategory(reader));
            }

            return categories;
        }

        public Category? GetCategoryById(int categoryId)
        {
            string query = @"
                SELECT c.CategoryID, c.Name, c.MenuCardID, mc.Name AS MenuCardName
                FROM Categories AS c
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                WHERE c.CategoryID = @CategoryID;";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId;
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapCategory(reader);
            }

            return null;
        }

        public List<MenuCard> GetMenuCards()
        {
            string query = @"
                SELECT MenuCardID, Name
                FROM MenuCards
                ORDER BY MenuCardID;";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            List<MenuCard> menuCards = new List<MenuCard>();
            while (reader.Read())
            {
                menuCards.Add(new MenuCard
                {
                    MenuCardID = reader.GetInt32(reader.GetOrdinal("MenuCardID")),
                    Name = reader.GetString(reader.GetOrdinal("Name"))
                });
            }

            return menuCards;
        }

        public void AddCategory(Category category)
        {
            string query = @"
                INSERT INTO Categories (Name, MenuCardID)
                OUTPUT INSERTED.CategoryID
                VALUES (@Name, @MenuCardID);";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            AddParameters(command, category);
            connection.Open();

            category.CategoryID = Convert.ToInt32(command.ExecuteScalar());
            _logger.LogInformation("Categorie toegevoegd: {CategoryId}.", category.CategoryID);
        }

        public void UpdateCategory(Category category)
        {
            string query = @"
                UPDATE Categories
                SET Name = @Name, MenuCardID = @MenuCardID
                WHERE CategoryID = @CategoryID;";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = category.CategoryID;
            AddParameters(command, category);
            connection.Open();

            if (command.ExecuteNonQuery() == 0)
            {
                throw new KeyNotFoundException(ErrorMessages.CategoryNotFound);
            }
        }

        private static void AddParameters(SqlCommand command, Category category)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = category.Name;
            command.Parameters.Add("@MenuCardID", SqlDbType.Int).Value = category.MenuCardID;
        }

        private static Category MapCategory(SqlDataReader reader)
        {
            int menuCardId = reader.GetInt32(reader.GetOrdinal("MenuCardID"));

            return new Category
            {
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                MenuCardID = menuCardId,
                MenuCard = new MenuCard
                {
                    MenuCardID = menuCardId,
                    Name = reader.GetString(reader.GetOrdinal("MenuCardName"))
                }
            };
        }
    }
}
