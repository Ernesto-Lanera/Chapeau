using System.ComponentModel.DataAnnotations;
using Chapeau.Constants;
using StockStatusEnum = Chapeau.Emums.StockStatus;

namespace Chapeau.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }

        [Required(ErrorMessage = ErrorMessages.MenuItemNameRequired)]
        [StringLength(100, ErrorMessage = ErrorMessages.MenuItemNameTooLong)]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = ErrorMessages.MenuItemPriceInvalid)]
        public decimal RetailPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = ErrorMessages.MenuItemStockNegative)]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = ErrorMessages.InvalidCategory)]
        public int CategoryID { get; set; }

        public Category Category { get; set; } = new();

        public string ImagePath { get; set; } = string.Empty;
        public bool IsAlcoholic { get; set; }

        public StockStatusEnum StockStatus => Stock switch
        {
            0 => StockStatusEnum.OutOfStock,
            <= 10 => StockStatusEnum.AlmostOutOfStock,
            _ => StockStatusEnum.Available
        };

        public bool IsOutOfStock => StockStatus == StockStatusEnum.OutOfStock;
        public bool IsAlmostOutOfStock => StockStatus == StockStatusEnum.AlmostOutOfStock;
        public bool CanChangeStock => IsActive;

        public string StockStatusText => StockStatus switch
        {
            StockStatusEnum.OutOfStock => "Uitverkocht",
            StockStatusEnum.AlmostOutOfStock => "Bijna op",
            _ => "Op voorraad"
        };

        public string StockStatusCssClass => StockStatus switch
        {
            StockStatusEnum.OutOfStock => "danger",
            StockStatusEnum.AlmostOutOfStock => "warn",
            _ => "ok"
        };

        public string StockDisplayStatusText => IsActive ? StockStatusText : "Inactief";
        public string StockDisplayStatusCssClass => IsActive ? StockStatusCssClass : "inactive";
        public string StockRowCssClass => !IsActive ? "inactive-stock-row" : IsOutOfStock ? "sold-out-row" : string.Empty;

        public void ChangeStock(int newStock)
        {
            if (newStock < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newStock), ErrorMessages.MenuItemStockNegative);
            }

            Stock = newStock;
        }
    }
}
