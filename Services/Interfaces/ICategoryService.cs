using quizweb.Models;
using quizweb.ViewModels.Category;

namespace quizweb.Services.Interfaces
{
    public interface ICategoryService
    {
         Task AddCategoryAsync(CategoryCreateViewModel category);
         Task UpdateCategoryAsync(Category category);
         Task DeleteCategoryAsync(int categoryId);
         Task<List<Category>> GetAllCategoryAsync();
         Task<Category> GetCategoryByIdAsync(int categoryId);


    }
}
