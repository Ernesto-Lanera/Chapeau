using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Emums;
using System.Linq;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
        ?? throw new Exception("Database connection string is missing.");
    }

    public List<Order> GetRunningOrders()
    {
        List<Order> orders = new List<Order>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = @"SELECT o.OrderID, o.TableID, t.TableNumber, o.GuestName, o.OrderDate, o.OrderStatus
                FROM Orders o
                JOIN Table_ t ON o.TableID = t.TableID
                WHERE o.OrderStatus IN (@Ordered, @BeingPrepared)
                ORDER BY o.OrderDate ASC";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Ordered", (int)OrderStatus.Ordered);
                command.Parameters.AddWithValue("@BeingPrepared", (int)OrderStatus.BeingPrepared);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(MapOrder(reader));
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

    public List<OrderItem> GetOrderItemsByOrderId(int orderId)
    {
        List<OrderItem> items = new List<OrderItem>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = @"SELECT oi.OrderItemID, oi.OrderID, oi.MenuItemID, oi.AmountOrdered,
                m.MenuItemID, m.Name, m.Price, m.Stock, m.IsActive, m.CategoryID, m.ImagePath, m.IsAlcoholic,
                oi.VATRate
                FROM OrderItem oi
                JOIN MenuItems m ON oi.MenuItemID = m.MenuItemID
                WHERE oi.OrderID = @OrderID";

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
            return items;
        }
    }

    private static string BuildTableStatusQuery()
    {
        return $@"SELECT t.TableID, t.TableNumber,
                o.OrderID, o.OrderStatus,
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
                TableNumber = tableNumber
            };
        }

        if (reader["OrderID"] != DBNull.Value)
        {
            tableDict[tableId].Orders.Add(new TableOrderInfo
            {
                OrderId = (int)reader["OrderID"],
                OrderStatus = (OrderStatus)(int)reader["OrderStatus"],
                HasFood = (bool)reader["HasFood"],
                HasDrink = (bool)reader["HasDrink"]
            });
        }
    }

    private static Order MapOrder(SqlDataReader reader)
    {
        return new Order
        {
            OrderId = (int)reader["OrderID"],
            TableId = (int)reader["TableID"],
            TableNumber = (int)reader["TableNumber"],
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
            MenuItem = menuItem,
            VATRate = (decimal)reader["VATRate"]
        };
    }
}