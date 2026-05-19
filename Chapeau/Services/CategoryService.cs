using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories.Category;
using Microsoft.Extensions.Logging;

namespace Chapeau.Services
{
    public class CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly ILogger<CategoryService> _logger = logger;

        // Haalt alle categorieën op
        public List<Category> GetCategories()
        {
            return _categoryRepository.GetCategories();
        }

        // Voegt nieuwe categorie toe
        public void AddCategory(Category category)
        {
            _categoryRepository.AddCategory(category);
            _logger.LogInformation("Category added: {CategoryName}", category.Name);
        }

        // Werkt categorie bij
        public void UpdateCategory(Category category)
        {
            _categoryRepository.UpdateCategory(category);
            _logger.LogInformation("Category updated: {CategoryId}", category.CategoryID);
        }

        // Telt aantal categorieën
        public int GetCategoryCount()
        {
            var categories = _categoryRepository.GetCategories();
            return categories.Count;
        }

        // Zoekt categorieën op naam
        public List<Category> SearchCategoriesByName(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _categoryRepository.GetCategories();

            var categories = _categoryRepository.GetCategories();
            return categories.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}