
namespace Someren.Models
{
    public class Drink
    {
        public int DrinkId { get; set; }

        public string DrinkName { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public bool Alcoholic { get; set; }
         public Drink(int drinkId, string drinkName, decimal price,int stock, bool alcoholic)
        {
            DrinkId = drinkId;
            DrinkName = drinkName;
            Price = price;
            Stock = stock;
            Alcoholic = alcoholic;
        }

        public Drink() { }

    }
}
