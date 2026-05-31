using Chapeau.Emums;

namespace Chapeau.Models
{
    public class OrderItem
    {
       public int OrderItemId { get; set; }
        public OrderStatus OrderItemStatus { get; set; }
        public CourseType? Course { get; set; }
        public int Amount { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; } 
        public decimal VATRate { get; set; }

        public string MenuItemName { get; set; } 
        public int MenuItemId { get; set; }

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
