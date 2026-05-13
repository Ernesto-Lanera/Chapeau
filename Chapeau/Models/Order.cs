namespace Someren.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int TableId { get; set; }
         
        public int TableNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public List<OrderItem> OrderParts {  get; set; }  


        public Order(int orderId, List<OrderItem> order, int tableId, DateTime date)
        {
            OrderId = orderId;
            OrderParts = order;
            TableId = tableId;
            OrderDate = date;
        }

            public Order() { }
    }
}
