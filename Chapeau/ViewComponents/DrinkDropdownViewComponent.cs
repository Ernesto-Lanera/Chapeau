using Microsoft.AspNetCore.Mvc;
using Someren.Repositories;

namespace Someren.ViewComponents
{
    public class DrinkDropdownViewComponent : ViewComponent
    {
        private readonly IDrinkRepository _drinkRepository;

        public DrinkDropdownViewComponent(IDrinkRepository drinkRepository)
        {
            _drinkRepository = drinkRepository;
        }

        public IViewComponentResult Invoke(int? selectedId = null)
        {
            var drinks = _drinkRepository.GetAll();

            ViewBag.SelectedId = selectedId;

            return View(drinks);
        }
    }
}