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
        void MarkOrderAsServed(int orderId);
    }
}