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
        public OrderStatus OrderStatus { get; set; }

        /// <summary>
        /// Collection of order items. This is the canonical collection; OrderParts is deprecated.
        /// </summary>
        public List<OrderItem> Items { get; set; } = new();

        /// <summary>
        /// Deprecated: Use Items instead. Kept for backward compatibility during transition.
        /// </summary>
        public List<OrderItem>? OrderParts
        {
            get => Items;
            set { if (value != null) Items = value; }
        }

        public Order() { }

        public Order(int orderId, List<OrderItem> order, int tableId, DateTime date)
        {
            OrderId = orderId;
            Items = order;
            TableId = tableId;
            OrderDate = date;
        }

        /// <summary>
        /// Add an item to the order.
        /// </summary>
        public void AddItem(OrderItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Items.Add(item);
        }

        /// <summary>
        /// Remove an item from the order.
        /// </summary>
        public void RemoveItem(OrderItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Items.Remove(item);
        }

        /// <summary>
        /// Total number of items in the order.
        /// </summary>
        public int TotalItemsCount => Items.Sum(i => i.Amount);

        /// <summary>
        /// Gross total (before VAT).
        /// </summary>
        public decimal GrossTotal => Items.Sum(i => i.GrossPrice);

        /// <summary>
        /// Low VAT (6%) total for the entire order.
        /// </summary>
        public decimal LowVATTotal => Items.Where(i => i.VATRate == 0.06m).Sum(i => i.VATAmount);

        /// <summary>
        /// High VAT (21%) total for the entire order.
        /// </summary>
        public decimal HighVATTotal => Items.Where(i => i.VATRate == 0.21m).Sum(i => i.VATAmount);

        /// <summary>
        /// Grand total VAT amount.
        /// </summary>
        public decimal TotalVAT => Items.Sum(i => i.VATAmount);

        /// <summary>
        /// Grand total including all VAT.
        /// </summary>
        public decimal GrandTotal => Items.Sum(i => i.TotalPrice);
    }
}
