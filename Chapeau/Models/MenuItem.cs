using System.ComponentModel.DataAnnotations;

namespace Chapeau.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }

        [Required(ErrorMessage = "Naam is verplicht")]
        [StringLength(100, ErrorMessage = "Naam mag niet langer zijn dan 100 karakters")]
        public string Name { get; set; } = "";

        [Range(0, double.MaxValue, ErrorMessage = "Inkoopprijs moet groter dan of gelijk aan 0 zijn")]
        public decimal PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Verkoopprijs moet groter dan of gelijk aan 0 zijn")]
        public decimal RetailPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Voorraad moet groter dan of gelijk aan 0 zijn")]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Selecteer een geldige categorie")]
        public int CategoryID { get; set; }

        public Category? Category { get; set; }

        public string StockStatus
        {
            get
            {
                if (Stock == 0) return "Uitverkocht";
                if (Stock <= 10) return "Bijna uitverkocht";
                return "Beschikbaar";
            }
        }
    }
}