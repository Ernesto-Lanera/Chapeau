using Chapeau.Emums;
using Chapeau.Models;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public interface IOrderService
    {
        Order GetActiveOrderByTableId(int tableId);
        List<Order> GetRunningOrders(OrderType type);
        List<Order> GetAllRunningOrders();
        TimeSpan GetWaitingTime(Order order);
        List<TableStatus> GetAllTableStatuses();
        void UpdateOrderStatus(int orderId, OrderStatus status);
        void UpdateOrderItemStatus(int orderItemId, OrderStatus status);
        void MarkOrderAsServed(int orderId);
        void UpdateOrderIfServed(int orderId);
        void UpdateAllOrderItemStatuses(int orderId, OrderType type, OrderStatus status);
        void UpdateCourseItemStatuses(int orderId, CourseType course, OrderStatus status);
        List<Order> GetFinishedOrdersToday(OrderType type);
    }
}