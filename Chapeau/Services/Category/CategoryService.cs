using Chapeau.Models;
using Chapeau.Repositories;

namespace Chapeau.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public List<Category> GetCategories()
        {
            return _categoryRepository.GetCategories();
        }

        public List<Category> GetCategoriesByCard(int? menuCardId)
        {
            if (!menuCardId.HasValue)
            {
                return GetCategories();
            }

            return _categoryRepository.GetCategoriesByCard(menuCardId.Value);
        }

        public Category? GetCategoryById(int categoryId)
        {
            return _categoryRepository.GetCategoryById(categoryId);
        }

        public int? GetValidCategoryId(int? menuCardId, int? categoryId)
        {
            if (!categoryId.HasValue)
            {
                return null;
            }

            Category? category = GetCategoryById(categoryId.Value);
            if (category == null)
            {
                return null;
            }

            if (menuCardId.HasValue && category.MenuCardID != menuCardId.Value)
            {
                return null;
            }

            return category.CategoryID;
        }

        public List<MenuCard> GetMenuCards()
        {
            return _categoryRepository.GetMenuCards();
        }
    }
}
