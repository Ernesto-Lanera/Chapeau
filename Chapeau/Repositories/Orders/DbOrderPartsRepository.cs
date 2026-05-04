using Microsoft.Data.SqlClient;
using Someren.Models;
using Microsoft.Extensions.Configuration;

namespace Someren.Repositories
{
    public class DbOrderPartsRepository : IOrderPartRepository
    {
        private readonly string _connectionString;

        public DbOrderPartsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SommerenDatabase")
                ?? throw new ArgumentNullException("Database connection string is missing!");
        }

        public void Add(OrderPart orderPart)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"
                            IF EXISTS (SELECT 1 FROM OrderParts WHERE orderid = @OrderId AND drinkid = @DrinkId)
                            BEGIN
                                UPDATE OrderParts 
                                SET amount = amount + @Amount 
                                WHERE orderid = @OrderId AND drinkid = @DrinkId;
                            END
                            ELSE
                            BEGIN
                                INSERT INTO OrderParts (orderid, drinkid, amount) 
                                VALUES (@OrderId, @DrinkId, @Amount);
                            END

                            UPDATE Drink 
                            SET Stock = Stock - @Amount 
                            WHERE DrinkId = @DrinkId;";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@OrderId", orderPart.OrderId);
            command.Parameters.AddWithValue("@DrinkId", orderPart.DrinkId);
            command.Parameters.AddWithValue("@Amount", orderPart.Amount);

            connection.Open();
            int nrOfRowsAffected = command.ExecuteNonQuery();
        }

        public void Delete(OrderPart orderPart)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                UPDATE Drink 
                SET Stock = Stock + (SELECT amount FROM OrderParts WHERE orderpartid = @OrderPartId)
                WHERE DrinkId = (SELECT drinkid FROM OrderParts WHERE orderpartid = @OrderPartId);

                DELETE FROM OrderParts WHERE orderpartid = @OrderPartId;";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@OrderPartId", orderPart.OrderPartId);

            connection.Open();
            int nrOfRowsAffected = command.ExecuteNonQuery();
            if (nrOfRowsAffected == 0) throw new InvalidOperationException($"No OrderParts found to delete with ID {orderPart.OrderPartId}!");
        }

        public OrderPart? GetById(int? id)
        {
            OrderPart? orderPart = null;
            using var connection = new SqlConnection(_connectionString);

            string query = "SELECT orderpartid, orderid, drinkid, amount FROM OrderParts WHERE orderpartid = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            command.Connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                orderPart = ReadOrderPart(reader);
            }

            return orderPart;
        }

        public void Update(OrderPart orderPart)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"
                            DECLARE @OldDrinkId INT, @OldAmount INT;
                            SELECT @OldDrinkId = drinkid, @OldAmount = amount 
                            FROM OrderParts 
                            WHERE orderpartid = @OrderPartId;
        
                            UPDATE Drink SET Stock = Stock + @OldAmount WHERE DrinkId = @OldDrinkId;
        
                            UPDATE Drink SET Stock = Stock - @Amount WHERE DrinkId = @DrinkId;
        
                            UPDATE OrderParts 
                            SET orderid = @OrderId, drinkid = @DrinkId, amount = @Amount 
                            WHERE orderpartid = @OrderPartId;";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@OrderId", orderPart.OrderId);
            command.Parameters.AddWithValue("@DrinkId", orderPart.DrinkId);
            command.Parameters.AddWithValue("@Amount", orderPart.Amount);
            command.Parameters.AddWithValue("@OrderPartId", orderPart.OrderPartId);

            connection.Open();
            command.ExecuteNonQuery();
        }

        private OrderPart ReadOrderPart(SqlDataReader reader)
        {
            int id = (int)reader["orderpartid"];
            int orderId = (int)reader["orderid"];
            int drinkId = (int)reader["drinkid"];
            int amount = (int)reader["amount"];

            return new OrderPart
            {
                OrderPartId = id,
                OrderId = orderId,
                DrinkId = drinkId,
                Amount = amount
            };
        }
    }
}