namespace Chapeau.Models
{
    /// <summary>
    /// JSON payload received from the client when submitting or updating an order.
    /// </summary>
    public class FinalOrderPayload
    {
        /// <summary>Table ID the order belongs to.</summary>
        public int TableId { get; set; }
        /// <summary>Order ID when updating an existing order; null for new orders.</summary>
        public int? OrderId { get; set; }
        /// <summary>Cart items to save.</summary>
        public List<JsCartItem> Items { get; set; }
    }
}
