using Chapeau.Emums;

namespace Chapeau.Models
{
    public class TableStatus
    {
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public bool IsManuallyOccupied { get; set; }
        public bool IsOccupied => Orders.Count > 0 || IsManuallyOccupied;
        public List<TableOrderInfo> Orders { get; set; } = new();
    }

    public class TableOrderInfo
    {
        public int OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool HasFood { get; set; }
        public bool HasDrink { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
