using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;

namespace Someren.ViewComponents
{
    public class StockAmountInputViewComponent : ViewComponent
    {
        private readonly IDrinkRepository _drinkRepository;

        public StockAmountInputViewComponent(IDrinkRepository drinkRepository)
        {
            _drinkRepository = drinkRepository;
        }

        public IViewComponentResult Invoke(int drinkId = 0, int currentAmount = 1)
        {
            int maxStock = 1;
            bool isDisabled = true;

            if (drinkId > 0)
            {
                Drink drink = _drinkRepository.GetById(drinkId)!;
                maxStock = drink != null ? drink.Stock : 1;
                isDisabled = false;
            }

            ViewBag.MaxStock = maxStock;
            ViewBag.CurrentAmount = currentAmount;
            ViewBag.IsDisabled = isDisabled;

            return View();
        }
    }
}