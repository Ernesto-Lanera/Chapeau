using Chapeau.Constants;
using Chapeau.Models;
using Chapeau.Repositories;
using Microsoft.Extensions.Logging;

namespace Chapeau.Services
{
    /// Service for category-related business logic.
    public class CategoryService(CategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        private readonly CategoryRepository _categoryRepository = categoryRepository;
        private readonly ILogger<CategoryService> _logger = logger;

        public List<Category> GetCategories()
        {
            return _categoryRepository.GetCategories();
        }

        /// Gets the count of categories.
        public int GetCategoryCount()
        {
            var categories = _categoryRepository.GetCategories();
            return categories.Count;
        }

        /// Searches categories by name.
        public List<Category> SearchCategoriesByName(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _categoryRepository.GetCategories();

            var categories = _categoryRepository.GetCategories();
            return categories.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}