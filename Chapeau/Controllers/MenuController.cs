using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chapeau.Services;
using Chapeau.Models;

namespace Chapeau.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;
        private readonly CategoryService _categoryService;

        public MenuController(MenuService menuService, CategoryService categoryService)
        {
            _menuService = menuService;
            _categoryService = categoryService; 
        }

        public IActionResult Index(int? cardId, int? categoryId)
        {
            var menuItems = _menuService.GetMenuItems(cardId, categoryId);
            ViewBag.SelectedCardId = cardId;
            ViewBag.SelectedCategoryId = categoryId;
            return View(menuItems);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var categories = _categoryService.GetCategories(); 
            ViewBag.Categories = new SelectList(categories, "CategoryID", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Create(MenuItem item)
        {
            if (ModelState.IsValid)
            {
                _menuService.AddMenuItem(item);
                return RedirectToAction(nameof(Index));
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                }
            }

            ViewBag.Categories = _categoryService.GetCategories();
            return View(item);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = _menuService.GetMenuItems(null, null).FirstOrDefault(m => m.MenuItemID == id);
            if (item == null) return NotFound();

            ViewBag.Categories = _categoryService.GetCategories();
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(MenuItem item)
        {
            if (ModelState.IsValid)
            {
                _menuService.UpdateMenuItem(item);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _categoryService.GetCategories();
            return View(item);
        }

        [HttpPost]
        public IActionResult ToggleActive(int id, bool active)
        {
            _menuService.SetMenuItemActive(id, active);
            return RedirectToAction(nameof(Index));
        }
    }
}