using Someren.Models;

namespace Someren.Repositories
{
    public interface IOrderPartRepository
    {
        void Add(OrderItem orderPart);
        void Update(OrderItem orderPart);
        void Delete(OrderItem orderPart);
        OrderItem GetById(int? id);
    }
}