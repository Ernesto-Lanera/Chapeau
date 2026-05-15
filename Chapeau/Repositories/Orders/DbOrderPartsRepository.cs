//using Microsoft.Data.SqlClient;
//using Chapeau.Models;
//using Microsoft.Extensions.Configuration;

//namespace Chapeau.Repositories
//{
//    public class DbOrderPartsRepository : IOrderPartRepository
//    {
//        private readonly string _connectionString;

//        public DbOrderPartsRepository(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("SommerenDatabase")
//                ?? throw new ArgumentNullException("Database connection string is missing!");
//        }

//        public void Add(OrderItem orderPart)
//        {
//            using var connection = new SqlConnection(_connectionString);
//            string query = @"
//                            IF EXISTS (SELECT 1 FROM OrderParts WHERE orderid = @OrderId AND MenuItemId = @MenuItemId)
//                            BEGIN
//                                UPDATE OrderParts 
//                                SET amount = amount + @Amount 
//                                WHERE orderid = @OrderId AND MenuItemId = @MenuItemId;
//                            END
//                            ELSE
//                            BEGIN
//                                INSERT INTO OrderParts (orderid, MenuItemId, amount) 
//                                VALUES (@OrderId, @MenuItemId, @Amount);
//                            END

//                            UPDATE Drink 
//                            SET Stock = Stock - @Amount 
//                            WHERE MenuItemId = @MenuItemId;";

//            using var command = new SqlCommand(query, connection);
//            command.Parameters.AddWithValue("@OrderId", orderPart.OrderId);
//            command.Parameters.AddWithValue("@MenuItemId", orderPart.MenuItemId);
//            command.Parameters.AddWithValue("@Amount", orderPart.Amount);

//            connection.Open();
//            int nrOfRowsAffected = command.ExecuteNonQuery();
//        }

//        public void Delete(OrderItem orderPart)
//        {
//            using var connection = new SqlConnection(_connectionString);

//            string query = @"
//                UPDATE Drink 
//                SET Stock = Stock + (SELECT amount FROM OrderParts WHERE orderpartid = @OrderPartId)
//                WHERE MenuItemId = (SELECT MenuItemId FROM OrderParts WHERE orderpartid = @OrderPartId);

//                DELETE FROM OrderParts WHERE orderpartid = @OrderPartId;";

//            using var command = new SqlCommand(query, connection);
//            command.Parameters.AddWithValue("@OrderPartId", orderPart.OrderPartId);

//            connection.Open();
//            int nrOfRowsAffected = command.ExecuteNonQuery();
//            if (nrOfRowsAffected == 0) throw new InvalidOperationException($"No OrderParts found to delete with ID {orderPart.OrderPartId}!");
//        }

//        public OrderItem? GetById(int? id)
//        {
//            OrderItem? orderPart = null;
//            using var connection = new SqlConnection(_connectionString);

//            string query = "SELECT orderpartid, orderid, MenuItemId, amount FROM OrderParts WHERE orderpartid = @Id";

//            using var command = new SqlCommand(query, connection);
//            command.Parameters.AddWithValue("@Id", id);

//            command.Connection.Open();
//            using SqlDataReader reader = command.ExecuteReader();

//            if (reader.Read())
//            {
//                orderPart = ReadOrderPart(reader);
//            }

//            return orderPart;
//        }

//        public void Update(OrderItem orderPart)
//        {
//            using var connection = new SqlConnection(_connectionString);
//            string query = @"
//                            DECLARE @OldMenuItemId INT, @OldAmount INT;
//                            SELECT @OldMenuItemId = MenuItemId, @OldAmount = amount 
//                            FROM OrderParts 
//                            WHERE orderpartid = @OrderPartId;
        
//                            UPDATE Drink SET Stock = Stock + @OldAmount WHERE MenuItemId = @OldMenuItemId;
        
//                            UPDATE Drink SET Stock = Stock - @Amount WHERE MenuItemId = @MenuItemId;
        
//                            UPDATE OrderParts 
//                            SET orderid = @OrderId, MenuItemId = @MenuItemId, amount = @Amount 
//                            WHERE orderpartid = @OrderPartId;";

//            using var command = new SqlCommand(query, connection);

//            command.Parameters.AddWithValue("@OrderId", orderPart.OrderId);
//            command.Parameters.AddWithValue("@MenuItemId", orderPart.MenuItemId);
//            command.Parameters.AddWithValue("@Amount", orderPart.Amount);
//            command.Parameters.AddWithValue("@OrderPartId", orderPart.OrderPartId);

//            connection.Open();
//            command.ExecuteNonQuery();
//        }

//        private OrderItem ReadOrderPart(SqlDataReader reader)
//        {
//            int id = (int)reader["orderpartid"];
//            int orderId = (int)reader["orderid"];
//            int MenuItemId = (int)reader["MenuItemId"];
//            int amount = (int)reader["amount"];

//            return new OrderItem
//            {
//                OrderPartId = id,
//                OrderId = orderId,
//                MenuItemId = MenuItemId,
//                Amount = amount
//            };
//        }
//    }
//}
//using Microsoft.Data.SqlClient;
//using Chapeau.Models;
//using Microsoft.Extensions.Configuration;

//namespace Chapeau.Repositories
//{
//    public class DbOrderPartsRepository : IOrderPartRepository
//    {
//        private readonly string _connectionString;

//        public DbOrderPartsRepository(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("SommerenDatabase")
//                ?? throw new ArgumentNullException("Database connection string is missing!");
//        }

//        public void Add(OrderItem orderPart)
//        {
//            using var connection = new SqlConnection(_connectionString);
//            string query = @"
//                            IF EXISTS (SELECT 1 FROM OrderParts WHERE orderid = @OrderId AND MenuItemId = @MenuItemId)
//                            BEGIN
//                                UPDATE OrderParts 
//                                SET amount = amount + @Amount 
//                                WHERE orderid = @OrderId AND MenuItemId = @MenuItemId;
//                            END
//                            ELSE
//                            BEGIN
//                                INSERT INTO OrderParts (orderid, MenuItemId, amount) 
//                                VALUES (@OrderId, @MenuItemId, @Amount);
//                            END

//                            UPDATE Drink 
//                            SET Stock = Stock - @Amount 
//                            WHERE MenuItemId = @MenuItemId;";

//            using var command = new SqlCommand(query, connection);
//            command.Parameters.AddWithValue("@OrderId", orderPart.OrderId);
//            command.Parameters.AddWithValue("@MenuItemId", orderPart.MenuItemId);
//            command.Parameters.AddWithValue("@Amount", orderPart.Amount);

//            connection.Open();
//            int nrOfRowsAffected = command.ExecuteNonQuery();
//        }

//        public void Delete(OrderItem orderPart)
//        {
//            using var connection = new SqlConnection(_connectionString);

//            string query = @"
//                UPDATE Drink 
//                SET Stock = Stock + (SELECT amount FROM OrderParts WHERE orderpartid = @OrderPartId)
//                WHERE MenuItemId = (SELECT MenuItemId FROM OrderParts WHERE orderpartid = @OrderPartId);

//                DELETE FROM OrderParts WHERE orderpartid = @OrderPartId;";

//            using var command = new SqlCommand(query, connection);
//            command.Parameters.AddWithValue("@OrderPartId", orderPart.OrderPartId);

//            connection.Open();
//            int nrOfRowsAffected = command.ExecuteNonQuery();
//            if (nrOfRowsAffected == 0) throw new InvalidOperationException($"No OrderParts found to delete with ID {orderPart.OrderPartId}!");
//        }

//        public OrderItem? GetById(int? id)
//        {
//            OrderItem? orderPart = null;
//            using var connection = new SqlConnection(_connectionString);

//            string query = "SELECT orderpartid, orderid, MenuItemId, amount FROM OrderParts WHERE orderpartid = @Id";

//            using var command = new SqlCommand(query, connection);
//            command.Parameters.AddWithValue("@Id", id);

//            command.Connection.Open();
//            using SqlDataReader reader = command.ExecuteReader();

//            if (reader.Read())
//            {
//                orderPart = ReadOrderPart(reader);
//            }

//            return orderPart;
//        }

//        public void Update(OrderItem orderPart)
//        {
//            using var connection = new SqlConnection(_connectionString);
//            string query = @"
//                            DECLARE @OldMenuItemId INT, @OldAmount INT;
//                            SELECT @OldMenuItemId = MenuItemId, @OldAmount = amount 
//                            FROM OrderParts 
//                            WHERE orderpartid = @OrderPartId;
        
//                            UPDATE Drink SET Stock = Stock + @OldAmount WHERE MenuItemId = @OldMenuItemId;
        
//                            UPDATE Drink SET Stock = Stock - @Amount WHERE MenuItemId = @MenuItemId;
        
//                            UPDATE OrderParts 
//                            SET orderid = @OrderId, MenuItemId = @MenuItemId, amount = @Amount 
//                            WHERE orderpartid = @OrderPartId;";

//            using var command = new SqlCommand(query, connection);

//            command.Parameters.AddWithValue("@OrderId", orderPart.OrderId);
//            command.Parameters.AddWithValue("@MenuItemId", orderPart.MenuItemId);
//            command.Parameters.AddWithValue("@Amount", orderPart.Amount);
//            command.Parameters.AddWithValue("@OrderPartId", orderPart.OrderPartId);

//            connection.Open();
//            command.ExecuteNonQuery();
//        }

//        private OrderItem ReadOrderPart(SqlDataReader reader)
//        {
//            int id = (int)reader["orderpartid"];
//            int orderId = (int)reader["orderid"];
//            int MenuItemId = (int)reader["MenuItemId"];
//            int amount = (int)reader["amount"];

//            return new OrderItem
//            {
//                OrderPartId = id,
//                OrderId = orderId,
//                MenuItemId = MenuItemId,
//                Amount = amount
//            };
//        }
//    }
//}