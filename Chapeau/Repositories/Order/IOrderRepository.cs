using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.Repositories
{
    public interface IOrderRepository
    {
        List<Order> GetRunningOrders();
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        Order GetActiveOrderByTableId(int tableId);
        List<TableStatus> GetAllTableStatuses();
        void UpdateOrderStatus(int orderId, OrderStatus newStatus);
    }
}