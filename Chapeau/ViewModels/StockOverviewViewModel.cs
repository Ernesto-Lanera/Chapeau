using Chapeau.Models;

namespace Chapeau.ViewModels
{
    public class StockOverviewViewModel
    {
        public IReadOnlyList<MenuItem> MenuItems { get; init; } = Array.Empty<MenuItem>();
        public IReadOnlyList<MenuCard> MenuCards { get; init; } = Array.Empty<MenuCard>();
        public IReadOnlyList<Category> Categories { get; init; } = Array.Empty<Category>();
        public int? SelectedCardId { get; init; }
        public int? SelectedCategoryId { get; init; }

        public int SoldOutCount => MenuItems.Count(item => item.IsActive && item.IsOutOfStock);
        public int AlmostOutCount => MenuItems.Count(item => item.IsActive && item.IsAlmostOutOfStock);
    }
}
