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
