using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class MenuManagementViewModel
    {
        public IReadOnlyList<MenuItem> MenuItems { get; init; } = Array.Empty<MenuItem>();
        public IReadOnlyList<MenuCard> MenuCards { get; init; } = Array.Empty<MenuCard>();
        public IReadOnlyList<Category> FilterCategories { get; init; } = Array.Empty<Category>();
        public IReadOnlyList<Category> FormCategories { get; init; } = Array.Empty<Category>();
        public int? SelectedCardId { get; init; }
        public int? SelectedCategoryId { get; init; }
        public bool ShowCreate { get; init; }
        public MenuItem? EditItem { get; init; }
        public int? EditMenuCardId => EditItem?.Category.MenuCardID;
    }

    public class MenuItemInputModel
    {
        public int MenuItemID { get; set; }
        public int MenuCardID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RetailPrice { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int CategoryID { get; set; }
        public bool IsAlcoholic { get; set; }
    }
}
