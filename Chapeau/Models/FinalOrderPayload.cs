namespace Chapeau.Models
{
    /// <summary>
    /// JSON payload received from the client when submitting or updating an order.
    /// </summary>
    public class FinalOrderPayload
    {
        /// <summary>Table ID the order belongs to.</summary>
        public int TableId { get; set; }
        public List<JsCartItem>? Items { get; set; }
    }
}
