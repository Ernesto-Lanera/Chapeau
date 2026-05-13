using Chapeau.Models;

namespace Chapeau.Repositories
{
    public interface IOrderRepository
    {
        List<Order> GetRunningOrders();
    }
}