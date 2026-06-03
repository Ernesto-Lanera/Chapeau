using System.Collections.Generic;
using Chapeau.Models;

namespace Chapeau.ViewModels
{
    /// <summary>
    /// View model for displaying payment details for an order.
    /// Uses domain-computed properties to ensure consistent calculation.
    /// </summary>
    public class PaymentOrderViewModel
    {
        public int OrderID { get; set; }
        public int TableNumber { get; set; }
        public IReadOnlyList<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Low VAT (6%) amount for display. Read-only; computed from items.
        /// </summary>
        public decimal LowVAT { get; set; }

        /// <summary>
        /// High VAT (21%) amount for display. Read-only; computed from items.
        /// </summary>
        public decimal HighVAT { get; set; }

        /// <summary>
        /// Grand total including VAT. Read-only; computed from items.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Gross total (before VAT). Computed from items.
        /// </summary>
        //public decimal GrossTotal => Items.Sum(i => i.GrossPrice);

        /// <summary>
        /// Total tip amount (if applicable). Default 0.
        /// </summary>
        public decimal TipAmount { get; set; } = 0;

        /// <summary>
        /// Grand total including tip.
        /// </summary>
        public decimal GrandTotalWithTip => Total + TipAmount;
    }
}