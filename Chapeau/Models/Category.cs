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
    }
}
