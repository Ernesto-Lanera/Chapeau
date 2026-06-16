using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class MenuManagementViewModel
    {
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public List<MenuCard> MenuCards { get; set; } = new List<MenuCard>();
        public List<Category> FilterCategories { get; set; } = new List<Category>();
        public List<Category> FormCategories { get; set; } = new List<Category>();
        public int? SelectedCardId { get; set; }
        public int? SelectedCategoryId { get; set; }
        public bool ShowCreate { get; set; }
        public MenuItem? EditItem { get; set; }

        public int? EditMenuCardId
        {
            get
            {
                if (EditItem == null)
                {
                    return null;
                }

                return EditItem.Category.MenuCardID;
            }
        }
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
