using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.Services
{
    public interface IOrderService
    {
       
        Order? GetServedOrderByTableId(int tableId);
     
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        List<Order> GetRunningOrders(OrderType type);

        List<Order> GetServedOrdersForPayment();
        TimeSpan GetWaitingTime(Order order);
        List<TableStatus> GetAllTableStatuses();
        void UpdateOrderStatus(int orderId, OrderStatus status);
        void UpdateOrderItemStatus(int orderItemId, OrderStatus status);
        void MarkOrderAsServed(int orderId);
        int MarkTableServed(int tableId);
        void UpdateOrderIfServed(int orderId);
        void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus status);
        void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus status);
        List<Order> GetFinishedOrdersToday(OrderType type);

        void SaveOrderToDb(Order order);
        List<Order> GetServedOrdersByTableId(int tableId);
    }
}
