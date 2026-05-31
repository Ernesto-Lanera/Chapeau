using Chapeau.Constants;

namespace Chapeau.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MenuCardID { get; set; }
        public MenuCard MenuCard { get; set; } = new();

        public string DisplayName =>
            MenuCardID == MenuCardConstants.DrinksCardId
                ? Name
                : $"{Name} ({MenuCardName})";

        public string MenuCardName =>
            !string.IsNullOrWhiteSpace(MenuCard.Name)
                ? MenuCard.Name
                : MenuCardConstants.GetMenuCardName(MenuCardID);

        /// Alleen drankcategorieen die alcohol kunnen bevatten mogen de 21%-btw keuze tonen.
        /// Frisdrank en Koffie / Thee zijn altijd niet-alcoholisch.
        public bool AllowsAlcoholicChoice =>
            MenuCardID == MenuCardConstants.DrinksCardId
            && !Name.Equals("Frisdrank", StringComparison.OrdinalIgnoreCase)
            && !Name.Replace(" ", string.Empty).Equals("Koffie/Thee", StringComparison.OrdinalIgnoreCase);
    }
}
