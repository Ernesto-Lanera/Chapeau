using System.Data;
using Chapeau.Constants;
using Chapeau.Models;
using Microsoft.Data.SqlClient;

namespace Chapeau.Repositories
{
    public class CategoryRepository(IConfiguration configuration, ILogger<CategoryRepository> logger) : ICategoryRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
            ?? throw new InvalidOperationException(ErrorMessages.ConnectionStringMissing);
        private readonly ILogger<CategoryRepository> _logger = logger;

        public List<Chapeau.Models.Category> GetCategories()
        {
            const string query = """
                SELECT c.CategoryID, c.Name, c.MenuCardID, mc.Name AS MenuCardName
                FROM Categories AS c
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                ORDER BY c.MenuCardID, c.Name;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            var categories = new List<Chapeau.Models.Category>();
            while (reader.Read())
            {
                categories.Add(MapCategory(reader));
            }

            return categories;
        }

        public Chapeau.Models.Category? GetCategoryById(int categoryId)
        {
            const string query = """
                SELECT c.CategoryID, c.Name, c.MenuCardID, mc.Name AS MenuCardName
                FROM Categories AS c
                INNER JOIN MenuCards AS mc ON mc.MenuCardID = c.MenuCardID
                WHERE c.CategoryID = @CategoryID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId;
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapCategory(reader) : null;
        }

        public void AddCategory(Chapeau.Models.Category category)
        {
            const string query = """
                INSERT INTO Categories (Name, MenuCardID)
                OUTPUT INSERTED.CategoryID
                VALUES (@Name, @MenuCardID);
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            AddParameters(command, category);
            connection.Open();

            category.CategoryID = Convert.ToInt32(command.ExecuteScalar());
            _logger.LogInformation("Categorie toegevoegd: {CategoryId}.", category.CategoryID);
        }

        public void UpdateCategory(Chapeau.Models.Category category)
        {
            const string query = """
                UPDATE Categories SET Name = @Name, MenuCardID = @MenuCardID
                WHERE CategoryID = @CategoryID;
                """;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(query, connection);
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = category.CategoryID;
            AddParameters(command, category);
            connection.Open();

            if (command.ExecuteNonQuery() == 0)
            {
                throw new KeyNotFoundException(ErrorMessages.CategoryNotFound);
            }
        }

        private static void AddParameters(SqlCommand command, Chapeau.Models.Category category)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = category.Name.Trim();
            command.Parameters.Add("@MenuCardID", SqlDbType.Int).Value = category.MenuCardID;
        }

        private static Chapeau.Models.Category MapCategory(SqlDataReader reader)
        {
            int cardId = reader.GetInt32(reader.GetOrdinal("MenuCardID"));
            return new Chapeau.Models.Category
            {
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                MenuCardID = cardId,
                MenuCard = new MenuCard
                {
                    MenuCardID = cardId,
                    Name = reader.GetString(reader.GetOrdinal("MenuCardName"))
                }
            };
        }
    }
}
