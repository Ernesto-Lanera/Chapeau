using Chapeau.Emums;

namespace Chapeau.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public string? GuestName { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public OrderType OrderType { get; set; }
        public Table_? Table { get; set; }
        public Employee? Employee { get; set; }

        public decimal Total => OrderItems?.Sum(i => i.Subtotal) ?? 0m;

        public Order() { }

        public Order(int orderId, List<OrderItem> orderitems, int tableId, DateTime date)
        {
            OrderId = orderId;
            OrderItems = orderitems;
            TableId = tableId;
            OrderDate = date;
        }

        public void AddItem(OrderItem item)
        {
            OrderItems ??= new List<OrderItem>();

            var existing = OrderItems.FirstOrDefault(i => i.MenuItemId == item.MenuItemId);
            if (existing is not null)
            {
                existing.AmountOrdered += item.AmountOrdered;
            }
            else
            {
                OrderItems.Add(item);
            }
        }

        public void RemoveItem(int menuItemId)
        {
            if (OrderItems is null) return;
            OrderItems.RemoveAll(i => i.MenuItemId == menuItemId);
        }

        public void UpdateItemQuantity(int menuItemId, int quantity)
        {
            var item = OrderItems?.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (item is not null)
            {
                item.AmountOrdered = quantity;
            }
        }

        public void UpdateItemComment(int menuItemId, string? comment)
        {
            var item = OrderItems?.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (item is not null)
            {
                item.Comment = string.IsNullOrEmpty(comment) ? null : comment;
            }
        }
    }
}
