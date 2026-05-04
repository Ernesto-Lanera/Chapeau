using Someren.Models;

namespace Someren.Repositories
{
    public interface IOrderPartRepository
    {
        void Add(OrderPart orderPart);
        void Update(OrderPart orderPart);
        void Delete(OrderPart orderPart);
        OrderPart GetById(int? id);
    }
}