namespace Chapeau.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int Amount { get; set; }
        public int OrderItemStatus { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal VATRate { get; set; }

        public int MenuItemId { get; set; }
        public int OrderId { get; set; }
        public string? Comment { get; set; }

        /// <summary>
        /// Alias for Amount for backward compatibility with repository code.
        /// </summary>
        public int AmountOrdered
        {
            get => Amount;
            set => Amount = value;
        }

        /// <summary>
        /// Total price including VAT for this line item.
        /// </summary>
        public decimal TotalPrice => Price * Amount * (1 + VATRate);

        /// <summary>
        /// VAT amount for this line item.
        /// </summary>
        public decimal VATAmount => Price * Amount * VATRate;

        /// <summary>
        /// Gross price (before VAT) for this line item.
        /// </summary>
        public decimal GrossPrice => Price * Amount;

        public OrderItem(int orderItemId, int menuItemId, int amount, int orderId)
        {
            OrderItemId = orderItemId;
            MenuItemId = menuItemId;
            Amount = amount;
            OrderId = orderId;
        }

        public OrderItem() { }
    }
}
