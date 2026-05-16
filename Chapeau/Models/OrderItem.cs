namespace Chapeau.Models
{
    public class OrderItem
    {
       public int OrderItemId { get; set; }
        public int AmountOrdered { get; set; }
        public int OrderItemStatus { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal VATRate { get; set; }
     
        //public int OrderPartId { get; set; }

        public int MenuItemId { get; set; }

        public int Amount { get; set; }

        public int OrderId { get; set; }

        public string? Comment { get; set; }

        public OrderItem(int orderItemId,  int menuItemId, int amount,int orderId)
        {
            OrderItemId = orderItemId;
            MenuItemId = menuItemId;
            Amount = amount;
            OrderId = orderId;
        }

        public OrderItem() { }

        
    }
}
