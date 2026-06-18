using Chapeau.Models;

namespace Chapeau.Services
{
    public interface ICategoryService
    {
        List<Category> GetCategories();
        List<Category> GetCategoriesByCard(int? menuCardId);
        Category? GetCategoryById(int categoryId);
        int? GetValidCategoryId(int? menuCardId, int? categoryId);
        List<MenuCard> GetMenuCards();
    }
}
