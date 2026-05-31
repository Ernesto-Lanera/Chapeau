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

        public Dictionary<int, OrderItem>? OrderItems { get; set; }

        public OrderStatus OrderStatus { get; set; }
        
        // payment voor carlo
        public List<OrderItem> Items { get; set; } = new();
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
        public decimal Total { get; set; }
    

        public Order() { }

        public Order(int orderId, Dictionary<int,OrderItem> orderitems, int tableId, DateTime date)
        {
            OrderId = orderId;
            OrderItems = orderitems;
            TableId = tableId;
            OrderDate = date;
        }

       
    }
}
