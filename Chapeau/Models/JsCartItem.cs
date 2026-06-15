namespace Chapeau.Models
{
    /// <summary>
    /// Represents a single item in the client-side shopping cart sent from the browser.
    /// </summary>
    public class JsCartItem
    {
        /// <summary>Menu item ID.</summary>
        public int MenuItemId { get; set; }
        /// <summary>Order item ID when editing an existing order item; null for new items.</summary>
        public int? OrderItemId { get; set; }
        /// <summary>Quantity ordered.</summary>
        public int Amount { get; set; }
        /// <summary>Optional comment for this item.</summary>
        public string? Comment { get; set; }
    }
}
