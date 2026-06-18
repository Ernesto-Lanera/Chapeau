using Chapeau.Models;

namespace Chapeau.Repositories
{
    public interface ICategoryRepository
    {
        List<Category> GetCategories();
        List<Category> GetCategoriesByCard(int menuCardId);
        Category? GetCategoryById(int categoryId);
        List<MenuCard> GetMenuCards();
        void AddCategory(Category category);
        void UpdateCategory(Category category);
    }
}
