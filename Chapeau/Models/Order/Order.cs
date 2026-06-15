using Chapeau.Emums;

namespace Chapeau.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public Table_? Table { get; set; }
        public string? GuestName { get; set; }
        public DateTime OrderDate { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public OrderType OrderType { get; set; }

        public List<OrderItem> Items { get; set; } = new();

        public List<OrderItem>? OrderItems
        {
            get => Items;
            set => Items = value ?? new List<OrderItem>();
        }

        public Order() { }

        public Order(int orderId, List<OrderItem> order, int tableId, DateTime date)
        {
            OrderId = orderId;
            Items = order ?? new List<OrderItem>();
            TableId = tableId;
            OrderDate = date;
        }

        public void AddItem(OrderItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Items.Add(item);
        }

        public void RemoveItem(OrderItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Items.Remove(item);
        }

        public int TotalItemsCount => Items.Sum(i => i.AmountOrdered);
        public decimal GrossTotal => Items.Sum(i => i.GrossPrice);
        public decimal LowVATTotal => Items.Where(i => i.VATRate == 0.06m).Sum(i => i.VATAmount);
        public decimal HighVATTotal => Items.Where(i => i.VATRate == 0.21m).Sum(i => i.VATAmount);
        public decimal TotalVAT => Items.Sum(i => i.VATAmount);
        public decimal GrandTotal => Items.Sum(i => i.TotalPrice);
    }
}
