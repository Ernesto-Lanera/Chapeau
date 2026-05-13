namespace Chapeau.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int TableID { get; set; }
        public int TableNumber { get; set; }
        public string? GuestName { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        
        // payment voor carlo
        public List<OrderItem> Items { get; set; } = new();
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
        public decimal Total { get; set; }
    }

    public enum OrderStatus
    {
        Ordered,
        BeingPrepared,
        ReadyToBeServed,
        Served,
        Paid
    }
}
