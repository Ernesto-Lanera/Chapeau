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

      

        public Order() { }

        public Order(int orderId, List<OrderItem> orderitems, int tableId, DateTime date)
        {
            OrderId = orderId;
            OrderItems = orderitems;
            TableId = tableId;
            OrderDate = date;
        }

       
    }
}
