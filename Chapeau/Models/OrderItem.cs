using Chapeau.Emums;

namespace Chapeau.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public OrderStatus OrderItemStatus { get; set; }
        public CourseType? Course { get; set; }
        public int AmountOrdered { get; set; }

        public string MenuItemName { get; set; } 
        public int MenuItemId { get; set; }

        public int OrderId { get; set; }

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

        public int MenuCardID { get; set; }

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
        public string? Comment;

        public OrderItem(int orderItemId, int menuItemId, int amount, int orderId)
        {
            OrderItemId = orderItemId;
            MenuItemId = menuItemId;
            AmountOrdered = amount;
            OrderId = orderId;
        }

        public OrderItem() { }
    }
}
