using Chapeau.Emums;
using Chapeau.Models;

namespace Chapeau.Services
{
    public interface IOrderService
    {
        Order GetActiveOrderByTableId(int tableId);
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        List<Order> GetRunningOrders(OrderType type);
        List<Order> GetAllRunningOrders();
        TimeSpan GetWaitingTime(Order order);
        List<TableStatus> GetAllTableStatuses();
        void MarkOrderAsServed(int orderId);
        void SavePayment(int orderId, int tableNumber, decimal tipAmount, string? feedback);
    }
}}