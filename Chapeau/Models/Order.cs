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

        
        /// Collection of order items. This is the canonical collection; OrderParts is deprecated.
        /// </summary>
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
