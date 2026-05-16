using Chapeau.Models;

namespace Chapeau.Repositories
{
    public interface IOrderRepository
    {
        List<Order> GetRunningOrders();
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        Order GetActiveOrderByTableId(int tableId);

    }
}