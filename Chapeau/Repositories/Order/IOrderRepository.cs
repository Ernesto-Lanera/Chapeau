using Chapeau.Emums;
using Chapeau.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chapeau.Repositories
{
    public interface IOrderRepository
    {
        List<Order> GetRunningOrders(OrderType type);
        List<OrderItem> GetOrderItemsByOrderId(int orderId, OrderType type);
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        Order? GetActiveOrderByTableId(int tableId);
        Order? GetServedOrderByTableId(int tableId);
        Order? GetOrderById(int orderId);
        List<Order> GetOrdersByStatus(OrderStatus status);
        List<TableStatus> GetAllTableStatuses();
        void UpdateOrderStatus(int orderId, OrderStatus newStatus);
        void UpdateOrderItemStatus(int orderItemId, OrderStatus newStatus);
        void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback);
        void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus newStatus);
        void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus newStatus);

        List<Order> GetFinishedOrdersToday(OrderType type);

        void SaveOrder(Order order);
    }
}