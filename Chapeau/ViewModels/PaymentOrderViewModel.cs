using System.Collections.Generic;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class PaymentOrderViewModel
    {
        public int OrderID { get; set; }
        public int TableNumber { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
        public decimal Total { get; set; }
    }
}