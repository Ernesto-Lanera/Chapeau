using System.ComponentModel.DataAnnotations;

namespace Chapeau.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = "";

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0")]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int CategoryID { get; set; }

        public Category? Category { get; set; }

        public string? ImagePath { get; set; }

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