namespace Someren.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int TableId { get; set; }

        public DateTime OrderDate { get; set; }

        public List<OrderPart> OrderParts {  get; set; }  

        public string? Note { get; set; } 

        public Order(int orderId, List<OrderPart> order, int tableId, DateTime date)
        {
            OrderId = orderId;
            OrderParts = order;
            TableId = tableId;
            OrderDate = date;
        }

            public Order() { }
    }
}
