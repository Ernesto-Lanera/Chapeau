using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Chapeau.Models;
using Chapeau.Repositories;
using Chapeau.Emums;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ChapeauDatabaseSQL")
        ?? throw new Exception("Database connection string is missing.");
    }
    //running orders van mart, niet aan komen carlo
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
                        Order order = new Order
                        {
                            OrderId = (int)reader["OrderID"],
                            TableId = (int)reader["TableID"],
                            TableNumber = (int)reader["TableNumber"],
                            GuestName = reader["GuestName"] == DBNull.Value ? null : reader["GuestName"].ToString(),
                            OrderDate = (DateTime)reader["OrderDate"],
                            OrderStatus = (OrderStatus)(int)reader["OrderStatus"]
                        };
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
                        return new Order
                        {
                            OrderID = (int)reader["OrderID"],
                            TableID = (int)reader["TableID"],
                            TableNumber = (int)reader["TableNumber"],
                            GuestName = reader["GuestName"] == DBNull.Value ? null : reader["GuestName"].ToString(),
                            OrderDate = (DateTime)reader["OrderDate"],
                            OrderStatus = (OrderStatus)(int)reader["OrderStatus"]
                        };
                    }
                    return null;
                }
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
                m.Name, m.Price, m.VATRate
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
                        OrderItem item = new OrderItem
                        {
                            OrderItemId = (int)reader["OrderItemID"],
                            OrderId = (int)reader["OrderID"],
                            MenuItemId = (int)reader["MenuItemID"],
                            AmountOrdered = (int)reader["AmountOrdered"],
                            Name = reader["Name"].ToString(),
                            Price = (decimal)reader["Price"],
                            VATRate = (decimal)reader["VATRate"]
                        };
                        items.Add(item);
                    }
                }
            }
            return items;
        }
    }

}