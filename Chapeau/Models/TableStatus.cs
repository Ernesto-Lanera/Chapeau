using Chapeau.Emums;

namespace Chapeau.Models
{
    /// <summary>
    /// Represents the current status of a table including its orders for the table overview display.
    /// </summary>
    public class TableStatus
    {
        /// <summary>Unique identifier for the table.</summary>
        public int TableId { get; set; }
        /// <summary>Display number of the table.</summary>
        public int TableNumber { get; set; }
        /// <summary>Whether staff manually marked this table as occupied.</summary>
        public bool IsManuallyOccupied { get; set; }
        /// <summary>Computed: true if the table has active orders or is manually marked occupied.</summary>
        public bool IsOccupied => Orders.Count > 0 || IsManuallyOccupied;
        /// <summary>Active (non-paid) orders at this table.</summary>
        public List<TableOrderInfo> Orders { get; set; } = new();
    }

    /// <summary>
    /// Lightweight order info used within the table overview to display per-order status.
    /// </summary>
    public class TableOrderInfo
    {
        /// <summary>Unique identifier for the order.</summary>
        public int OrderId { get; set; }
        /// <summary>Current status of the order (Ordered, BeingPrepared, etc.).</summary>
        public OrderStatus OrderStatus { get; set; }
        /// <summary>Whether the order contains food items.</summary>
        public bool HasFood { get; set; }
        /// <summary>Whether the order contains drink items.</summary>
        public bool HasDrink { get; set; }
        /// <summary>When the order was placed.</summary>
        public DateTime OrderDate { get; set; }
    }
}
