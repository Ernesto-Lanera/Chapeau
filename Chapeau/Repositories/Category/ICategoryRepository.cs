using System.Collections.Generic;
using Chapeau.Models;

namespace Chapeau.Repositories.Category
{
    public interface ICategoryRepository
    {
        List<Chapeau.Models.Category> GetCategories();
        void AddCategory(Chapeau.Models.Category category);
        void UpdateCategory(Chapeau.Models.Category category);
    }
}