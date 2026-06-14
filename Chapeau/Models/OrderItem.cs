using Chapeau.Emums;

namespace Chapeau.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public OrderStatus OrderItemStatus { get; set; }
        public CourseType? Course { get; set; }
        public int Amount { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = new();

        public int OrderId { get; set; }
        public string? Comment { get; set; }

        public string MenuItemName { get; set; } = string.Empty;

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

        public decimal GrossPrice => Price * AmountOrdered;
        public decimal VATAmount => Price * AmountOrdered * VATRate;
        public decimal TotalPrice => Price * AmountOrdered * (1 + VATRate);

        public decimal Subtotal => GrossPrice;
        public decimal TotalWithVat => Subtotal + VATAmount;

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