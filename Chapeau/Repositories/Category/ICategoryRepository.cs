using Chapeau.Models;

namespace Chapeau.Repositories
{
    public interface ICategoryRepository
    {
        List<Chapeau.Models.Category> GetCategories();
        Chapeau.Models.Category? GetCategoryById(int categoryId);
        void AddCategory(Chapeau.Models.Category category);
        void UpdateCategory(Chapeau.Models.Category category);
    }
}
