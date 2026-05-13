using Chapeau.Models;

namespace Chapeau.Services
{
    public interface IOrderService
    {
        List<Order> GetRunningOrders();
        TimeSpan GetWaitingTime(Order order);
    }
}