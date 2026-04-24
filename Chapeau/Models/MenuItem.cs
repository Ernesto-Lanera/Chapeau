namespace Chapeau.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
        public int CategoryID { get; set; }

        public string StockStatus
        {
            get
            {
                if (Stock == 0) return "Out of stock";
                if (Stock <= 10) return "Almost out of stock";
                return "Available";
            }
        }
    }
}