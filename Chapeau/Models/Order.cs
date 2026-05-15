using Chapeau.Emums;

namespace Chapeau.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int TableId { get; set; }

        public int TableNumber { get; set; }
        public string GuestName { get; set; }
        public DateTime OrderDate { get; set; }

        public List<OrderItem> OrderParts { get; set; }

        public OrderStatus Status { get; set; }

        public Order() { }

        public Order(int orderId, List<OrderItem> order, int tableId, DateTime date)
        {
            OrderId = orderId;
            OrderParts = order;
            TableId = tableId;
            OrderDate = date;
        }

       
    }
}
