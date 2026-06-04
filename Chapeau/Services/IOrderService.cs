using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.Services
{
    public interface IOrderService
    {
        Order? GetActiveOrderByTableId(int tableId);
        Order? GetServedOrderByTableId(int tableId);
        Order? GetOrderById(int orderId);
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        List<Order> GetRunningOrders(OrderType type);
        List<Order> GetAllRunningOrders();
        List<Order> GetServedOrdersForPayment();
        TimeSpan GetWaitingTime(Order order);
        List<TableStatus> GetAllTableStatuses();
        void UpdateOrderStatus(int orderId, OrderStatus status);
        void UpdateOrderItemStatus(int orderItemId, OrderStatus status);
        void MarkOrderAsServed(int orderId);
        void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback);
        void UpdateOrderIfServed(int orderId);
        void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus status);
        void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus status);
        List<Order> GetFinishedOrdersToday(OrderType type);

        Order MakeNewOrder(int tableId);
        Order AddOrderItemToOrder(int menuItemId, Order order, string menuItemName);
        Order RemoveItemFromOrder(int menuItemId, Order order);
        Order UpdateItemFromOrder(int menuItemId, Order order, int newAmount);
        Order ChangeCommentInItem(int menuItemId, Order order, string comment);
        void SaveOrderToDb(Order order);
    }
}