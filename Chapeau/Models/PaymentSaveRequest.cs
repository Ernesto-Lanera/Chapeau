namespace Chapeau.Models
{
    public class PaymentSaveRequest
    {
        public int OrderId { get; set; }
        public int TableNumber { get; set; }
        public decimal TipAmount { get; set; }
        public string? Feedback { get; set; }
    }
}
