using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.ViewModels.Category;

namespace QuizWeb_TrioForce.Services.Interfaces
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
