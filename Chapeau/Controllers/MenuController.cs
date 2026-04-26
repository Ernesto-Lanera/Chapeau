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
            if (_menuService.GetMenuItems(null, null).Any(m => m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "A menu item with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _menuService.AddMenuItem(item);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving to the database.");
                }
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
            if (_menuService.GetMenuItems(null, null).Any(m => m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase) && m.MenuItemID != item.MenuItemID))
            {
                ModelState.AddModelError("Name", "Another menu item with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _menuService.UpdateMenuItem(item);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving to the database.");
                }
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