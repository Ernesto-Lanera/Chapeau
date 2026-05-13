namespace Chapeau.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int TableID { get; set; }
        public int TableNumber { get; set; }
        public string GuestName { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
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
