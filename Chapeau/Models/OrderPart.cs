namespace Someren.Models
{
    public class OrderPart
    {
        public int OrderPartId { get; set; }

        public int? DrinkId { get; set; }

        public int MenuItemId { get; set; }

        public int Amount { get; set; }

        public int OrderId { get; set; }

        public OrderPart(int orderPartId,  int drinkId, int amount,int orderId)
        {
            OrderPartId = orderPartId;
            DrinkId = drinkId;
            Amount = amount;
            OrderId = orderId;
        }

        public OrderPart() { }

        
    }
}
