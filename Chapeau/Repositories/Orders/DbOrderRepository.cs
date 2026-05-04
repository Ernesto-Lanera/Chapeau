using Microsoft.Data.SqlClient;
using Someren.Models;

namespace Someren.Repositories
{
    public class DbOrdersRepository : IOrderRepository
    {
        private readonly string? _connectionString;

        public DbOrdersRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SommerenDatabase");
        }

        public int Add(Order order)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"INSERT INTO [Orders] (tableId, orderdate) 
                             OUTPUT INSERTED.orderid 
                             VALUES (@tableId, @OrderDate)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@tableId", order.TableId);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate == default ? DateTime.Now : order.OrderDate);

            command.Connection.Open();
            return (int)command.ExecuteScalar();
        }

        public void Delete(Order order)
        {
            using var connection = new SqlConnection(_connectionString);

    
            string query = @"
                            UPDATE Drink
                            SET Stock = Stock + OrderTotals.TotalAmount
                             FROM Drink
                            INNER JOIN (
                                SELECT drinkid, SUM(amount) as TotalAmount
                                FROM OrderParts
                                WHERE orderid = @OrderId
                                GROUP BY drinkid
                            ) OrderTotals ON Drink.DrinkId = OrderTotals.drinkid;

                            DELETE FROM OrderParts WHERE orderid = @OrderId;
                            DELETE FROM Orders WHERE orderid = @OrderId;
         ";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@OrderId", order.OrderId);

            command.Connection.Open();
            int nrOfRowsAffected = command.ExecuteNonQuery();

            if (nrOfRowsAffected == 0)
            {
                throw new Exception("No records were found to delete or update!");
            }
        }

        public List<Order> GetAll()
        {
            List<Order> orders = new List<Order>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT orderid, tableId, orderdate FROM [Orders]", connection);

            command.Connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                orders.Add(ReadOrder(reader));
            }
            return orders;
        }

        public Order? GetById(int orderId)
        {
            Order? order = null;
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT orderid, tableId, orderdate FROM [Orders] WHERE orderid = @OrderId", connection);
            command.Parameters.AddWithValue("@OrderId", orderId);

            command.Connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                order = ReadOrder(reader);
            }
            return order;
        }

        public void Update(Order order)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = @"UPDATE [Orders] 
                             SET tableId = @tableId, orderdate = @OrderDate 
                             WHERE orderid = @OrderId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@tableId", order.TableId);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            command.Parameters.AddWithValue("@OrderId", order.OrderId);

            command.Connection.Open();
            int nrOfRowsAffected = command.ExecuteNonQuery();
            if (nrOfRowsAffected == 0) throw new Exception("No records Updated!");
        }

        private Order ReadOrder(SqlDataReader reader)
        {
            int id = (int)reader["orderid"];
            int tableId = (int)reader["tableId"];
            DateTime orderDate = (DateTime)reader["orderdate"];
            List<OrderPart> orderParts = FillOrderParts(id);

            return new Order { OrderId = id, TableId = tableId, OrderDate = orderDate, OrderParts = orderParts };
        }

        private List<OrderPart> FillOrderParts(int orderId)
        {
            List<OrderPart> parts = new List<OrderPart>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT orderpartid, orderid, drinkid, amount FROM OrderParts Where orderid = @OrderId ", connection);

            command.Parameters.AddWithValue("@OrderId", orderId);
            command.Connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                parts.Add(ReadOrderPart(reader));
            }
            return parts;
        }

        private OrderPart ReadOrderPart(SqlDataReader reader)
        {
            int id = (int)reader["orderpartid"];
            int orderId = (int)reader["orderid"];
            int drinkId = (int)reader["drinkid"];
            int amount = (int)reader["amount"];

            return new OrderPart { OrderPartId = id, OrderId = orderId, DrinkId = drinkId, Amount = amount };
        }
    }
}