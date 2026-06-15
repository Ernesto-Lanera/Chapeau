using Chapeau.Emums;

namespace Chapeau.Models
{
    /// <summary>
    /// Represents a single line item within an order, linking a menu item with quantity, comment, and status.
    /// </summary>
    public class OrderItem
    {
        /// <summary>Unique identifier for this order item.</summary>
        public int OrderItemId { get; set; }
        /// <summary>Current preparation status of this item.</summary>
        public OrderStatus OrderItemStatus { get; set; }
        /// <summary>Course type (Starter, Main, Dessert) derived from the menu item category.</summary>
        public CourseType? Course { get; set; }
        /// <summary>Quantity ordered.</summary>
        public int Amount { get; set; }

        /// <summary>Foreign key to the menu item.</summary>
        public int MenuItemId { get; set; }
        /// <summary>Navigation property to the menu item details.</summary>
        public MenuItem MenuItem { get; set; } = new();

        /// <summary>Foreign key to the parent order.</summary>
        public int OrderId { get; set; }
        /// <summary>Optional special instructions or remarks for this item.</summary>
        public string? Comment { get; set; }

        /// <summary>Display name of the menu item (denormalized for convenience).</summary>
        public string MenuItemName { get; set; } = string.Empty;

        /// <summary>Gets or sets the item name from the underlying menu item.</summary>
        public string Name
        {
            get => MenuItem.Name;
            set => MenuItem.Name = value;
        }

        /// <summary>Gets or sets the price from the underlying menu item.</summary>
        public decimal Price
        {
            get => MenuItem.RetailPrice;
            set => MenuItem.RetailPrice = value;
        }

        /// <summary>VAT rate applied (0.06 for non-alcoholic, 0.21 for alcoholic).</summary>
        public decimal VATRate { get; set; }

        /// <summary>Alias for Amount, used for backward compatibility.</summary>
        public int AmountOrdered
        {
            get => Amount;
            set => Amount = value;
        }

        /// <summary>Total price before VAT (Price * Quantity).</summary>
        public decimal GrossPrice => Price * AmountOrdered;
        /// <summary>VAT amount (Price * Quantity * VATRate).</summary>
        public decimal VATAmount => Price * AmountOrdered * VATRate;
        /// <summary>Total price including VAT.</summary>
        public decimal TotalPrice => Price * AmountOrdered * (1 + VATRate);

        /// <summary>Alias for GrossPrice.</summary>
        public decimal Subtotal => GrossPrice;
        /// <summary>Alias for GrossPrice + VATAmount.</summary>
        public decimal TotalWithVat => Subtotal + VATAmount;

        /// <summary>Creates an order item with the given parameters.</summary>
        public OrderItem(int orderItemId, int menuItemId, int amount, int orderId)
        {
            OrderItemId = orderItemId;
            MenuItemId = menuItemId;
            Amount = amount;
            OrderId = orderId;
        }

        /// <summary>Creates an empty order item.</summary>
        public OrderItem() { }
    }
}