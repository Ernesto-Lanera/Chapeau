using Chapeau.Models;

namespace Chapeau.Services
{
    public interface ICategoryService
    {
        List<Category> GetCategories();
        void AddCategory(Category category);
        void UpdateCategory(Category category);
        int GetCategoryCount();
        List<Category> SearchCategoriesByName(string searchTerm);
    }
}
