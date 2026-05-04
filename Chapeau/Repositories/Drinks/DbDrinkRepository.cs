using Microsoft.Data.SqlClient;
using Someren.Models;

namespace Someren.Repositories
{
    public class DbDrinksRepository : IDrinkRepository
    {
        private readonly string? _connectionString;

        public DbDrinksRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SommerenDatabase");
        }

        public void Add(Drink drink)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"INSERT INTO Drink (drinkname, price, stock, alcoholic) 
                             VALUES (@Name, @Price, @Stock, @Alcoholic)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", drink.DrinkName);
            command.Parameters.AddWithValue("@Price", drink.Price);
            command.Parameters.AddWithValue("@Stock", drink.Stock);
            command.Parameters.AddWithValue("@Alcoholic", drink.Alcoholic);

            command.Connection.Open();
            int nrOfRowsAffected = command.ExecuteNonQuery();
            if (nrOfRowsAffected != 1) throw new Exception("No Drink Added!");
        }

        public void Delete(Drink drink)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("DELETE FROM Drink WHERE drinkid = @DrinkId", connection);
            command.Parameters.AddWithValue("@DrinkId", drink.DrinkId);

            command.Connection.Open();
            int nrOfRowsAffected = command.ExecuteNonQuery();
            if (nrOfRowsAffected == 0) throw new Exception("No records deleted!");
        }

        public List<Drink> GetAll()
        {
            List<Drink> drinks = new List<Drink>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT drinkid, drinkname, price, stock, alcoholic FROM Drink", connection);

            command.Connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                drinks.Add(ReadDrink(reader));
            }
            return drinks;
        }

        public Drink? GetById(int drinkId)
        {
            Drink? drink = null;
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT drinkid, drinkname, price, stock, alcoholic FROM Drink WHERE drinkid = @DrinkId", connection);
            command.Parameters.AddWithValue("@DrinkId", drinkId);

            command.Connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                drink = ReadDrink(reader);
            }
            return drink;
        }

        public void Update(Drink drink)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"UPDATE Drink 
                             SET drinkname = @Name, price = @Price, stock = @Stock, alcoholic = @Alcoholic 
                             WHERE drinkid = @DrinkId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", drink.DrinkName);
            command.Parameters.AddWithValue("@Price", drink.Price);
            command.Parameters.AddWithValue("@Stock", drink.Stock);
            command.Parameters.AddWithValue("@Alcoholic", drink.Alcoholic);
            command.Parameters.AddWithValue("@DrinkId", drink.DrinkId);

            command.Connection.Open();
            int nrOfRowsAffected = command.ExecuteNonQuery();
            if (nrOfRowsAffected == 0) throw new Exception("No records Updated!");
        }

        private Drink ReadDrink(SqlDataReader reader)
        {
            int id = (int)reader["drinkid"];
            string name = reader["drinkname"] != DBNull.Value ? (string)reader["drinkname"] : string.Empty;
            decimal price = reader["price"] != DBNull.Value ? (decimal)reader["price"] : 0m;
            int stock = reader["stock"] != DBNull.Value ? (int)reader["stock"] : 0;
            bool alcoholic = reader["alcoholic"] != DBNull.Value && (bool)reader["alcoholic"];

            return new Drink { DrinkId = id, DrinkName = name, Price = price, Stock = stock, Alcoholic = alcoholic };
        }
    }
}