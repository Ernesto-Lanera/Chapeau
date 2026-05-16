namespace Chapeau.Models
{
    public class OrderItem
    {
        public int OrderPartId { get; set; }

        public int MenuItemId { get; set; }

        public int Amount { get; set; }

        public int OrderId { get; set; }

        public string? Comment { get; set; }

        public OrderItem(int orderPartId,  int menuItemId, int amount,int orderId)
        {
            OrderPartId = orderPartId;
            MenuItemId = menuItemId;
            Amount = amount;
            OrderId = orderId;
        }

        public OrderItem() { }

        
    }
}
