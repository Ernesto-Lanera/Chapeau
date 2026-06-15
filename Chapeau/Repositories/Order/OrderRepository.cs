using Chapeau.Constants;
using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.Repositories;
using Microsoft.Data.SqlClient;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
        ?? throw new Exception("Database connection string is missing.");
    }

    public List<Order> GetRunningOrders(OrderType type)
    {
        List<Order> orders = new List<Order>();

        string typeFilter = type == OrderType.Food
            ? "AND c.MenuCardID IN (@FoodCard1, @FoodCard2)"
            : "AND c.MenuCardID = @DrinkCard";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = $@"
            SELECT o.OrderID, o.TableID, t.TableNumber, o.GuestName, o.OrderDate, o.OrderStatus
            FROM Orders o
            JOIN Table_ t ON o.TableID = t.TableID
            WHERE o.OrderStatus NOT IN (@ReadyToBeServed, @Served, @Paid)
            AND EXISTS (
                SELECT 1 FROM OrderItem oi
                JOIN MenuItems m ON m.MenuItemID = oi.MenuItemID
                JOIN Categories c ON c.CategoryID = m.CategoryID
                WHERE oi.OrderID = o.OrderID {typeFilter}
            )
            AND EXISTS (
                SELECT 1 FROM OrderItem oi
                JOIN MenuItems m ON m.MenuItemID = oi.MenuItemID
                JOIN Categories c ON c.CategoryID = m.CategoryID
                WHERE oi.OrderID = o.OrderID
                AND oi.OrderItemStatus != @ReadyToBeServed
                {typeFilter}
            )
            ORDER BY o.OrderDate ASC";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ReadyToBeServed", (int)OrderStatus.ReadyToBeServed);
                command.Parameters.AddWithValue("@Served", (int)OrderStatus.Served);
                command.Parameters.AddWithValue("@Paid", (int)OrderStatus.Paid);
                command.Parameters.AddWithValue("@FoodCard1", MenuCardConstants.FoodMenuCard);
                command.Parameters.AddWithValue("@FoodCard2", MenuCardConstants.FoodMenuCard2);
                command.Parameters.AddWithValue("@DrinkCard", MenuCardConstants.DrinkMenuCard);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = MapOrder(reader);
                        order.OrderItems = GetOrderItemsByOrderId(order.OrderId, type);
                        orders.Add(order);
                    }
                }
            }
        }
        return orders;
    }

    public Order? GetActiveOrderByTableId(int tableId)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = @"SELECT o.OrderID, o.TableID, t.TableNumber, o.GuestName, o.OrderDate, o.OrderStatus
                FROM Orders o
                JOIN Table_ t ON o.TableID = t.TableID
                WHERE o.TableID = @TableID
                AND o.OrderStatus IN (@Ordered, @BeingPrepared)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TableID", tableId);
                command.Parameters.AddWithValue("@Ordered", (int)OrderStatus.Ordered);
                command.Parameters.AddWithValue("@BeingPrepared", (int)OrderStatus.BeingPrepared);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapOrder(reader);
                    }
                    return null;
                }
            }
        }
    }

    public Order? GetServedOrderByTableId(int tableId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = @"SELECT o.OrderID, o.TableID, t.TableNumber, o.GuestName, o.OrderDate, o.OrderStatus
            FROM Orders o
            JOIN Table_ t ON o.TableID = t.TableID
            WHERE o.TableID = @TableID
            AND o.OrderStatus = @Served";

        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TableID", tableId);
        command.Parameters.AddWithValue("@Served", (int)OrderStatus.Served);

        using SqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? MapOrder(reader) : null;
    }

    public Order? GetOrderById(int orderId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = @"SELECT o.OrderID, o.TableID, t.TableNumber, o.GuestName, o.OrderDate, o.OrderStatus
            FROM Orders o
            JOIN Table_ t ON o.TableID = t.TableID
            WHERE o.OrderID = @OrderID";

        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@OrderID", orderId);

        using SqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? MapOrder(reader) : null;
    }

    public List<Order> GetOrdersByStatus(OrderStatus status)
    {
        List<Order> orders = new List<Order>();

        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = @"SELECT o.OrderID, o.TableID, t.TableNumber, o.GuestName, o.OrderDate, o.OrderStatus
        FROM Orders o
        JOIN Table_ t ON o.TableID = t.TableID
        WHERE o.OrderStatus = @Status
        ORDER BY o.OrderDate ASC";

        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Status", (int)status);

        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            orders.Add(MapOrder(reader));
        }

        foreach (var order in orders)
        {
            order.OrderItems = GetOrderItemsByOrderId(order.OrderId);
        }

        return orders;
    }

    public List<TableStatus> GetAllTableStatuses()
    {
        Dictionary<int, TableStatus> tableDict = new Dictionary<int, TableStatus>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = BuildTableStatusQuery();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Paid", (int)OrderStatus.Paid);
                command.Parameters.AddWithValue("@MinTable", MenuCardConstants.MinTableNumber);
                command.Parameters.AddWithValue("@MaxTable", MenuCardConstants.MaxTableNumber);
                command.Parameters.AddWithValue("@FoodCard1", MenuCardConstants.FoodMenuCard);
                command.Parameters.AddWithValue("@FoodCard2", MenuCardConstants.FoodMenuCard2);
                command.Parameters.AddWithValue("@DrinkCard", MenuCardConstants.DrinkMenuCard);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MapTableRow(reader, tableDict);
                    }
                }
            }
        }
        return tableDict.Values.OrderBy(t => t.TableNumber).ToList();
    }

    public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "UPDATE Orders SET OrderStatus = @Status WHERE OrderID = @OrderID";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Status", (int)newStatus);
                command.Parameters.AddWithValue("@OrderID", orderId);
                command.ExecuteNonQuery();
            }
        }
    }

    public List<OrderItem> GetOrderItemsByOrderId(int orderId, OrderType type)
    {
        List<OrderItem> items = [];
        
        string typeFilter = type == OrderType.Food
            ? "AND c.MenuCardID IN (@FoodCard1, @FoodCard2)"
            : "AND c.MenuCardID = @DrinkCard";

        string query = $@"
        SELECT oi.OrderItemID, oi.OrderID, oi.MenuItemID, oi.AmountOrdered, 
               oi.Comment, oi.OrderItemStatus, m.Name,
        CASE 
        WHEN m.CategoryID IN (34, 37) THEN 0
        WHEN m.CategoryID IN (36, 39) THEN 1
        WHEN m.CategoryID IN (35, 38) THEN 2
        WHEN m.CategoryID IN (8, 9) THEN 3
        ELSE 0
        END AS Course
        FROM OrderItem oi
        JOIN MenuItems m ON m.MenuItemID = oi.MenuItemID
        JOIN Categories c ON c.CategoryID = m.CategoryID
        WHERE oi.OrderID = @OrderID {typeFilter}";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@OrderID", orderId);
                command.Parameters.AddWithValue("@FoodCard1", MenuCardConstants.FoodMenuCard);
                command.Parameters.AddWithValue("@FoodCard2", MenuCardConstants.FoodMenuCard2);
                command.Parameters.AddWithValue("@DrinkCard", MenuCardConstants.DrinkMenuCard);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var menuItem = new MenuItem
                        {
                            MenuItemID = (int)reader["MenuItemID"],
                            Name = (string)reader["Name"]
                        };

                        OrderItem orderItem = new OrderItem
                        {
                            OrderItemId = (int)reader["OrderItemID"],
                            OrderId = (int)reader["OrderID"],
                            MenuItemId = menuItem.MenuItemID,
                            AmountOrdered = (int)reader["AmountOrdered"],
                            Comment = reader["Comment"] as string,
                            OrderItemStatus = (OrderStatus)reader["OrderItemStatus"],
                            MenuItemName = menuItem.Name,
                            MenuItem = menuItem,
                            Course = reader["Course"] == DBNull.Value ? null : (CourseType?)(int)reader["Course"]
                        };

                        items.Add(orderItem);
                    }
                }
            }
        }

        return items;
    }

    public List<OrderItem> GetOrderItemsByOrderId(int orderId)
    {
        List<OrderItem> items = new List<OrderItem>();

        string query = @"
        SELECT oi.OrderItemID, oi.OrderID, oi.MenuItemID, oi.AmountOrdered, 
               oi.Comment, oi.OrderItemStatus, m.Name, ISNULL(m.Price, 0) AS Price, m.Stock, m.IsActive, m.CategoryID, m.ImagePath, ISNULL(m.IsAlcoholic, 0) AS IsAlcoholic
        FROM OrderItem oi
        JOIN MenuItems m ON m.MenuItemID = oi.MenuItemID
        WHERE oi.OrderID = @OrderID";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@OrderID", orderId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapOrderItem(reader));
                    }
                }
            }
        }
        return items;
    }

    private static string BuildTableStatusQuery()
    {
        return $@"SELECT t.TableID, t.TableNumber, t.IsManuallyOccupied,
                o.OrderID, o.OrderStatus, o.OrderDate,
                CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM OrderItem oi
                    JOIN MenuItems m ON m.MenuItemID = oi.MenuItemID
                    JOIN Categories c ON c.CategoryID = m.CategoryID
                    WHERE oi.OrderID = o.OrderID AND c.MenuCardID IN (@FoodCard1, @FoodCard2)
                ) THEN 1 ELSE 0 END AS BIT) as HasFood,
                CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM OrderItem oi
                    JOIN MenuItems m ON m.MenuItemID = oi.MenuItemID
                    JOIN Categories c ON c.CategoryID = m.CategoryID
                    WHERE oi.OrderID = o.OrderID AND c.MenuCardID = @DrinkCard
                ) THEN 1 ELSE 0 END AS BIT) as HasDrink
            FROM Table_ t
            LEFT JOIN Orders o ON o.TableID = t.TableID AND o.OrderStatus <> @Paid
            WHERE t.TableNumber BETWEEN @MinTable AND @MaxTable
            ORDER BY t.TableNumber, o.OrderDate";
    }

    private static void MapTableRow(SqlDataReader reader, Dictionary<int, TableStatus> tableDict)
    {
        int tableId = (int)reader["TableID"];
        int tableNumber = (int)reader["TableNumber"];

        if (!tableDict.ContainsKey(tableId))
        {
            tableDict[tableId] = new TableStatus
            {
                TableId = tableId,
                TableNumber = tableNumber,
                IsManuallyOccupied = (bool)reader["IsManuallyOccupied"]
            };
        }

        if (reader["OrderID"] != DBNull.Value)
        {
            tableDict[tableId].Orders.Add(new TableOrderInfo
            {
                OrderId = (int)reader["OrderID"],
                OrderStatus = (OrderStatus)(int)reader["OrderStatus"],
                HasFood = (bool)reader["HasFood"],
                HasDrink = (bool)reader["HasDrink"],
                OrderDate = reader["OrderDate"] != DBNull.Value ? (DateTime)reader["OrderDate"] : DateTime.MinValue
            });
        }
    }

    private static Order MapOrder(SqlDataReader reader)
    {
        var table = new Table_
        {
            TableId = (int)reader["TableID"],
            TableNumber = (int)reader["TableNumber"]
        };

        return new Order
        {
            OrderId = (int)reader["OrderID"],
            TableId = table.TableId,
            TableNumber = table.TableNumber,
            Table = table,
            GuestName = reader["GuestName"] == DBNull.Value ? null : reader["GuestName"].ToString(),
            OrderDate = (DateTime)reader["OrderDate"],
            OrderStatus = (OrderStatus)(int)reader["OrderStatus"]
        };
    }

    private static OrderItem MapOrderItem(SqlDataReader reader)
    {
        var menuItem = new MenuItem
        {
            MenuItemID = (int)reader["MenuItemID"],
            Name = reader["Name"].ToString() ?? string.Empty,
            RetailPrice = (decimal)reader["Price"],
            Stock = (int)reader["Stock"],
            IsActive = (bool)reader["IsActive"],
            CategoryID = (int)reader["CategoryID"],
            ImagePath = reader["ImagePath"].ToString() ?? string.Empty,
            IsAlcoholic = (bool)reader["IsAlcoholic"]
        };

        return new OrderItem
        {
            OrderItemId = (int)reader["OrderItemID"],
            OrderId = (int)reader["OrderID"],
            MenuItemId = (int)reader["MenuItemID"],
            AmountOrdered = (int)reader["AmountOrdered"],
            MenuItemName = reader["Name"].ToString() ?? string.Empty,
            Comment = reader["Comment"] == DBNull.Value ? null : reader["Comment"].ToString(),
            OrderItemStatus = (OrderStatus)(int)reader["OrderItemStatus"],
            MenuItem = menuItem,
            VATRate = GetVatRate(menuItem.IsAlcoholic)
        };
    }

    private static decimal GetVatRate(bool isAlcoholic)
    {
        return isAlcoholic ? 0.21m : 0.06m;
    }

        public int MarkReadyOrdersAsServed(int tableId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"UPDATE Orders SET OrderStatus = @Served
                WHERE TableID = @TableID AND OrderStatus = @ReadyToBeServed";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Served", (int)OrderStatus.Served);
            command.Parameters.AddWithValue("@TableID", tableId);
            command.Parameters.AddWithValue("@ReadyToBeServed", (int)OrderStatus.ReadyToBeServed);

            return command.ExecuteNonQuery();
        }

        public int InsertOrder(Order order)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            string query = @"INSERT INTO Orders (TableID, GuestName, OrderDate, OrderStatus)
                             OUTPUT INSERTED.OrderID
                             VALUES (@TableID, @GuestName, @OrderDate, @OrderStatus)";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TableID", order.TableId);
            command.Parameters.AddWithValue("@GuestName", (object?)order.GuestName ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            command.Parameters.AddWithValue("@OrderStatus", (int)OrderStatus.Ordered);
            return (int)command.ExecuteScalar();
        }

        public void InsertOrderItems(int orderId, List<OrderItem> items)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            foreach (var item in items)
            {
                string query = @"INSERT INTO OrderItem (OrderID, MenuItemID, AmountOrdered, Comment, OrderItemStatus)
                                 VALUES (@OrderID, @MenuItemID, @AmountOrdered, @Comment, @OrderItemStatus)";
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderID", orderId);
                command.Parameters.AddWithValue("@MenuItemID", item.MenuItemId);
                command.Parameters.AddWithValue("@AmountOrdered", item.AmountOrdered);
                command.Parameters.AddWithValue("@Comment", (object?)item.Comment ?? DBNull.Value);
                command.Parameters.AddWithValue("@OrderItemStatus", (int)OrderStatus.Ordered);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateOrderItemStatus(int orderItemId, OrderStatus newStatus)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "UPDATE OrderItem SET OrderItemStatus = @Status WHERE OrderItemID = @OrderItemID";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Status", (int)newStatus);
                command.Parameters.AddWithValue("@OrderItemID", orderItemId);
                command.ExecuteNonQuery();
            }
        }
    }

    public void UpdateItemComment(int orderItemId, string? comment)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = "UPDATE OrderItem SET Comment = @Comment WHERE OrderItemID = @OrderItemID";

        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Comment", (object?)comment ?? DBNull.Value);
        command.Parameters.AddWithValue("@OrderItemID", orderItemId);
        command.ExecuteNonQuery();
    }

    public void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int tableId;

            string orderInfoQuery = "SELECT TableID, OrderStatus FROM Orders WHERE OrderID = @OrderID";
            using (SqlCommand command = new SqlCommand(orderInfoQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@OrderID", orderId);

                using SqlDataReader reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    throw new InvalidOperationException("Order not found.");
                }

                tableId = (int)reader["TableID"];
                var status = (OrderStatus)(int)reader["OrderStatus"];

                if (status != OrderStatus.Served && status != OrderStatus.Paid)
                {
                    throw new InvalidOperationException("Order is not served yet.");
                }
            }

            string insertPaymentQuery = @"
                INSERT INTO Payment (OrderID, TableID, TotalTipAmount, Feedback)
                VALUES (@OrderID, @TableID, @TipAmount, @Feedback)";

            using (SqlCommand command = new SqlCommand(insertPaymentQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@OrderID", orderId);
                command.Parameters.AddWithValue("@TableID", tableId);
                command.Parameters.AddWithValue("@TipAmount", tipAmount);
                command.Parameters.AddWithValue("@Feedback", (object?)feedback ?? DBNull.Value);
                command.ExecuteNonQuery();
            }

            string updateOrderQuery = "UPDATE Orders SET OrderStatus = @Paid WHERE OrderID = @OrderID";
            using (SqlCommand command = new SqlCommand(updateOrderQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@Paid", (int)OrderStatus.Paid);
                command.Parameters.AddWithValue("@OrderID", orderId);
                command.ExecuteNonQuery();
            }

            string updateTableQuery = "UPDATE Table_ SET IsManuallyOccupied = 0 WHERE TableID = @TableID";
            using (SqlCommand command = new SqlCommand(updateTableQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@TableID", tableId);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus newStatus)
    {
        List<OrderItem> items = GetOrderItemsByOrderId(orderId, type)
            .Where(i => (int)i.OrderItemStatus < (int)newStatus)
            .ToList();
        foreach (var item in items)
            UpdateOrderItemStatus(item.OrderItemId, newStatus);
    }

    public void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus newStatus)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = @"
            UPDATE oi SET oi.OrderItemStatus = @Status
            FROM OrderItem oi
            JOIN MenuItems m ON m.MenuItemID = oi.MenuItemID
            WHERE oi.OrderID = @OrderID
            AND oi.OrderItemStatus < @Status
            AND (
                (@Course = 0 AND m.CategoryID IN (34, 37)) OR
                (@Course = 1 AND m.CategoryID IN (36, 39)) OR
                (@Course = 2 AND m.CategoryID IN (35, 38)) OR
                (@Course = 3 AND m.CategoryID IN (8, 9))
            )";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Status", (int)newStatus);
                command.Parameters.AddWithValue("@OrderID", orderId);
                command.Parameters.AddWithValue("@Course", (int)course);
                command.ExecuteNonQuery();
            }
        }   
    }

    public List<Order> GetFinishedOrdersToday(OrderType type)
    {
        List<Order> orders = new List<Order>();

        string typeFilter = type == OrderType.Food
            ? "AND c.MenuCardID IN (@FoodCard1, @FoodCard2)"
            : "AND c.MenuCardID = @DrinkCard";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = $@"
            SELECT o.OrderID, o.TableID, t.TableNumber, o.GuestName, o.OrderDate, o.OrderStatus
            FROM Orders o
            JOIN Table_ t ON o.TableID = t.TableID
            WHERE o.OrderStatus = @ReadyToBeServed
            OR o.OrderStatus = @Served
            OR o.OrderStatus = @Paid
            AND CAST(o.OrderDate AS DATE) = CAST(GETDATE() AS DATE)
            AND EXISTS (
                SELECT 1 FROM OrderItem oi
                JOIN MenuItems m ON m.MenuItemID = oi.MenuItemID
                JOIN Categories c ON c.CategoryID = m.CategoryID
                WHERE oi.OrderID = o.OrderID {typeFilter}
            )
            ORDER BY o.OrderDate DESC";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ReadyToBeServed", (int)OrderStatus.ReadyToBeServed);
                command.Parameters.AddWithValue("@Served", (int)OrderStatus.Served);
                command.Parameters.AddWithValue("@Paid", (int)OrderStatus.Paid);
                command.Parameters.AddWithValue("@FoodCard1", MenuCardConstants.FoodMenuCard);
                command.Parameters.AddWithValue("@FoodCard2", MenuCardConstants.FoodMenuCard2);
                command.Parameters.AddWithValue("@DrinkCard", MenuCardConstants.DrinkMenuCard);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = MapOrder(reader);
                        order.OrderItems = GetOrderItemsByOrderId(order.OrderId, type);
                        orders.Add(order);
                    }
                }
            }
        }
        return orders;
    }

    public void SaveOrder(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = @"INSERT INTO Orders (tableid, orderdate, GuestName, orderstatus) 
                     OUTPUT INSERTED.OrderID
                     VALUES (@TableId, @OrderDate, @GuestName, @OrderStatus);";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TableId", order.TableId);
        command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
        command.Parameters.AddWithValue("@GuestName", (object?)order.GuestName ?? DBNull.Value);
        command.Parameters.AddWithValue("@OrderStatus", (int)OrderStatus.Ordered);

        int newOrderId = (int)command.ExecuteScalar();

        if (order.OrderItems != null)
        {
            foreach (var item in order.OrderItems)
            {
                item.OrderId = newOrderId;
                SaveOrderItems(item, connection);
            }
        }
    }

    private void SaveOrderItems(OrderItem item, SqlConnection connection)
    {
        string query = @"INSERT INTO OrderItem (orderid, MenuItemid, AmountOrdered, comment, OrderItemStatus) 
                     VALUES (@OrderId, @MenuItemId, @AmountOrdered, @Comment, @OrderItemStatus); 
                     UPDATE MenuItems SET Stock = Stock - @AmountOrdered
                     WHERE MenuItemId = @MenuItemId;";

        using var command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@OrderId", item.OrderId);
        command.Parameters.AddWithValue("@MenuItemId", item.MenuItemId);
        command.Parameters.AddWithValue("@AmountOrdered", item.AmountOrdered);
        command.Parameters.AddWithValue("@OrderItemStatus", (int)OrderStatus.Ordered);
        command.Parameters.AddWithValue("@Comment", (object?)item.Comment ?? DBNull.Value);

        command.ExecuteNonQuery();
    }

    public List<Order> GetOrdersByTableNumber(int tableNumber)
    {
        List<Order> orders = new List<Order>();

        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = @"SELECT o.OrderID, o.TableID, t.TableNumber, o.GuestName, o.OrderDate, o.OrderStatus
        FROM Orders o
        JOIN Table_ t ON o.TableID = t.TableID
        WHERE t.TableNumber = @TableNumber
        AND o.OrderStatus = @Served
        ORDER BY o.OrderDate ASC";

        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TableNumber", tableNumber);
        command.Parameters.AddWithValue("@Served", (int)OrderStatus.Served);

        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
            orders.Add(MapOrder(reader));

        foreach (var order in orders)
            order.Items = GetOrderItemsByOrderId(order.OrderId);

        return orders;
    }
}