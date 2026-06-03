using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.Repositories
{
    public interface IOrderRepository
    {
        List<Order> GetRunningOrders(OrderType type);
        List<OrderItem> GetOrderItemsByOrderId(int orderId, OrderType type);
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        Order GetActiveOrderByTableId(int tableId);
        List<TableStatus> GetAllTableStatuses();
        void UpdateOrderStatus(int orderId, OrderStatus newStatus);
        void UpdateOrderItemStatus(int orderItemId, OrderStatus newStatus);
        void SaveOrder(Order order);
    }
}