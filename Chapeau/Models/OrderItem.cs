namespace Chapeau.Models
{
    public class OrderItem
    {
       public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string Comment { get; set; }
        public int AmountOrdered { get; set; }
        public int OrderItemStatus { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal VATRate { get; set; }
     
        public enum OrderStatus
        {
            Ordered,
            BeingPrepared,
            ReadyToBeServed,
            Served,
            Paid
        }
    }
}
