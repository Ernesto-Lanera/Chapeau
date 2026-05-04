using Microsoft.AspNetCore.Mvc;
using Someren.Repositories;

namespace Someren.ViewComponents
{
    public class DrinkFieldViewComponent : ViewComponent
    {
        private readonly IDrinkRepository _DrinkRepository;

    public DrinkFieldViewComponent(IDrinkRepository DrinkRepository)
    {
        _DrinkRepository = DrinkRepository;
    }

    public IViewComponentResult Invoke(int selectedId)
    {
        var Drink = _DrinkRepository.GetById(selectedId);

        return View(Drink);
    }
}
}
