using System.Collections.Generic;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class PaymentOrderViewModel
    {
        public int OrderID { get; set; }
        public List<int> OrderIDs { get; set; } = new List<int>();
        public int TableNumber { get; set; }
        public IReadOnlyList<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
        public decimal Total { get; set; }
        public decimal TipAmount { get; set; } = 0;
        public decimal GrandTotalWithTip => Total + TipAmount;
    }
}