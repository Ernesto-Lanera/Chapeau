using Chapeau.Models;
using Chapeau.ViewModels;

namespace Chapeau.Services
{
    public interface IStockService
    {
        StockOverviewViewModel GetOverview(int? cardId, int? categoryId);
        MenuItem ChangeStock(int menuItemId, int newStock);
    }
}
