using Chapeau.Emums;

namespace Chapeau.Models
{
    /// <summary>
    /// Represents a customer order placed at a table, containing order items and computed totals.
    /// </summary>
    public class Order
    {
        /// <summary>Unique identifier for the order.</summary>
        public int OrderId { get; set; }
        /// <summary>Foreign key to the table where the order was placed.</summary>
        public int TableId { get; set; }
        /// <summary>Display number of the table.</summary>
        public int TableNumber { get; set; }
        /// <summary>Navigation property to the table.</summary>
        public Table_? Table { get; set; }
        /// <summary>Optional guest name associated with the order.</summary>
        public string? GuestName { get; set; }
        /// <summary>Date and time when the order was placed.</summary>
        public DateTime OrderDate { get; set; }

        /// <summary>Current status of the order (Ordered, BeingPrepared, etc.).</summary>
        public OrderStatus OrderStatus { get; set; }
        /// <summary>Whether this order contains Food or Drink items.</summary>
        public OrderType OrderType { get; set; }

        /// <summary>Line items belonging to this order.</summary>
        public List<OrderItem> Items { get; set; } = new();

        /// <summary>Backward-compatible alias for Items.</summary>
        public List<OrderItem>? OrderItems
        {
            get => Items;
            set => Items = value ?? new List<OrderItem>();
        }

        /// <summary>Creates an empty order.</summary>
        public Order() { }

        /// <summary>Creates an order with the given parameters.</summary>
        public Order(int orderId, List<OrderItem> order, int tableId, DateTime date)
        {
            OrderId = orderId;
            Items = order ?? new List<OrderItem>();
            TableId = tableId;
            OrderDate = date;
        }

        /// <summary>Adds an item to the order.</summary>
        public void AddItem(OrderItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Items.Add(item);
        }

        /// <summary>Removes an item from the order.</summary>
        public void RemoveItem(OrderItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Items.Remove(item);
        }

        /// <summary>Total number of items (sum of all amounts).</summary>
        public int TotalItemsCount => Items.Sum(i => i.AmountOrdered);
        /// <summary>Total price before VAT.</summary>
        public decimal GrossTotal => Items.Sum(i => i.GrossPrice);
        /// <summary>Total VAT at the low rate (6%, non-alcoholic).</summary>
        public decimal LowVATTotal => Items.Where(i => i.VATRate == 0.06m).Sum(i => i.VATAmount);
        /// <summary>Total VAT at the high rate (21%, alcoholic).</summary>
        public decimal HighVATTotal => Items.Where(i => i.VATRate == 0.21m).Sum(i => i.VATAmount);
        /// <summary>Total VAT amount across all items.</summary>
        public decimal TotalVAT => Items.Sum(i => i.VATAmount);
        /// <summary>Grand total including VAT.</summary>
        public decimal GrandTotal => Items.Sum(i => i.TotalPrice);
    }
}
