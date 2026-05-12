using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chapeau.Services;
using Chapeau.Models;

namespace Chapeau.Controllers
{
    public class MenuController(MenuService menuService, CategoryService categoryService) : Controller
    {
        private readonly MenuService _menuService = menuService;
        private readonly CategoryService _categoryService = categoryService;

        public IActionResult Index(int? cardId, int? categoryId)
        {
            var menuItems = _menuService.GetMenuItems(cardId, categoryId);
            var categories = _categoryService.GetCategories();

            ViewBag.SelectedCardId = cardId;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Categories = categories;

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
            // Check for duplicate name (case-insensitive)
            if (_menuService.GetMenuItems(null, null).Any(m =>
                    m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "A menu item with this name already exists.");
            }

            // Additional server-side validation
            if (item.Price < 0)
            {
                ModelState.AddModelError("Price", "Price cannot be negative.");
            }

            if (item.Stock < 0)
            {
                ModelState.AddModelError("Stock", "Stock cannot be negative.");
            }

            if (item.CategoryID <= 0)
            {
                ModelState.AddModelError("CategoryID", "Please select a valid category.");
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
                catch (Exception)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving to the database.");
                }
            }

            // Repopulate categories for form re-display
            ViewBag.Categories = new SelectList(_categoryService.GetCategories(), "CategoryID", "Name", item.CategoryID);
            return View(item);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = _menuService.GetMenuItems(null, null)
                .FirstOrDefault(m => m.MenuItemID == id);

            if (item == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_categoryService.GetCategories(), "CategoryID", "Name", item.CategoryID);
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(MenuItem item)
        {
            // Check for duplicate name (case-insensitive, excluding current item)
            if (_menuService.GetMenuItems(null, null).Any(m =>
                    m.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)
                    && m.MenuItemID != item.MenuItemID))
            {
                ModelState.AddModelError("Name", "Another menu item with this name already exists.");
            }

            // Additional server-side validation
            if (item.Price < 0)
            {
                ModelState.AddModelError("Price", "Price cannot be negative.");
            }

            if (item.Stock < 0)
            {
                ModelState.AddModelError("Stock", "Stock cannot be negative.");
            }

            if (item.CategoryID <= 0)
            {
                ModelState.AddModelError("CategoryID", "Please select a valid category.");
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
                catch (Exception)
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving to the database.");
                }
            }

            // Repopulate categories for form re-display
            ViewBag.Categories = new SelectList(_categoryService.GetCategories(), "CategoryID", "Name", item.CategoryID);
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