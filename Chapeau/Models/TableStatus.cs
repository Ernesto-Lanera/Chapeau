using Chapeau.Emums;

namespace Chapeau.Models
{
    public class TableStatus
    {
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public bool IsOccupied { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public int? OrderId { get; set; }
    }
}
