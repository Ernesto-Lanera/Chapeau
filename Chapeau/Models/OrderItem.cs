namespace Chapeau.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int Amount { get; set; }
        public int OrderItemStatus { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = new();

        public int OrderId { get; set; }
        public string? Comment { get; set; }

        public string Name
        {
            get => MenuItem.Name;
            set => MenuItem.Name = value;
        }

        public decimal Price
        {
            get => MenuItem.RetailPrice;
            set => MenuItem.RetailPrice = value;
        }

        public decimal VATRate { get; set; }
        public int AmountOrdered
        {
            get => Amount;
            set => Amount = value;
        }
        public decimal TotalPrice => Price * Amount * (1 + VATRate);
        public decimal VATAmount => Price * Amount * VATRate;
        public decimal GrossPrice => Price * Amount;

        public OrderItem(int orderItemId, int menuItemId, int amount, int orderId)
        {
            OrderItemId = orderItemId;
            MenuItemId = menuItemId;
            Amount = amount;
            OrderId = orderId;
        }

        public OrderItem() { }
    }
}
