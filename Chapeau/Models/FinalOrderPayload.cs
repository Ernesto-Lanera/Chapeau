namespace Chapeau.Models
{
    public class FinalOrderPayload
    {
        public int TableId { get; set; }
        public List<JsCartItem> Items { get; set; }
    }
}
