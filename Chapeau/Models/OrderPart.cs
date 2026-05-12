namespace Someren.Models
{
    public class OrderPart
    {
        public int OrderPartId { get; set; }

        public int MenuItemId { get; set; }

        public int Amount { get; set; }

        public int OrderId { get; set; }

        public OrderPart(int orderPartId,  int menuItemId, int amount,int orderId)
        {
            OrderPartId = orderPartId;
            MenuItemId = menuItemId;
            Amount = amount;
            OrderId = orderId;
        }

        public OrderPart() { }

        
    }
}
