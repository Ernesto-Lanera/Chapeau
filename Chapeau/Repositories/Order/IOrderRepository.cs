using Chapeau.Emums;
using Chapeau.Models;

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
        void UpdateItemComment(int orderItemId, string? comment);
        void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback);
        void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus newStatus);
        void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus newStatus);

        List<Order> GetFinishedOrdersToday(OrderType type);

        int MarkReadyOrdersAsServed(int tableId);
        void SaveOrder(Order order);
        List<Order> GetOrdersByTableNumber(int tableNumber);
    }
}
