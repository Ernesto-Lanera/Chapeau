using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class StockOverviewViewModel
    {
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public List<MenuCard> MenuCards { get; set; } = new List<MenuCard>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public int? SelectedCardId { get; set; }
        public int? SelectedCategoryId { get; set; }

        public int SoldOutCount
        {
            get { return MenuItems.Count(item => item.IsActive && item.IsOutOfStock); }
        }

        public int AlmostOutCount
        {
            get { return MenuItems.Count(item => item.IsActive && item.IsAlmostOutOfStock); }
        }
    }
}
