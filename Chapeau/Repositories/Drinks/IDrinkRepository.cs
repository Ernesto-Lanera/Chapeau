using Someren.Models;

namespace Someren.Repositories
{
    public interface IDrinkRepository
    {
        List<Drink> GetAll();
        Drink? GetById(int DrinkId);
        void Add(Drink drink);
        void Update(Drink drink);
        void Delete(Drink drink);
    }
}
