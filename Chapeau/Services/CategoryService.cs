using System.Collections.Generic;
using Chapeau.Models;
using Chapeau.Repositories;

namespace Chapeau.Services
{
    public class CategoryService(CategoryRepository categoryRepository)
    {
        private readonly CategoryRepository _categoryRepository = categoryRepository;

        public List<Category> GetCategories()
        {
            return _categoryRepository.GetCategories();
        }
    }
}