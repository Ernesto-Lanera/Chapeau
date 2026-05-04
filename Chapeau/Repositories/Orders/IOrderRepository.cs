using Someren.Models;

namespace Someren.Repositories
{
    public interface IOrderRepository
    {
        List<Order> GetAll();
        Order? GetById(int orderId);
        int Add(Order order);
        void Update(Order order);
        void Delete(Order order);
    }
}