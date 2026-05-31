using System.Collections.Generic;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class PaymentOrderViewModel
    {
        public int OrderID { get; set; }
        public int TableNumber { get; set; }
        public IReadOnlyList<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
        public decimal Total { get; set; }
    }
}