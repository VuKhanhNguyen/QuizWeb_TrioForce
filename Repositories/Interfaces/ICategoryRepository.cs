using quizweb.Models;

namespace quizweb.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllCategoryAsync();
        Task<Category?> GetCategoryByIdAsync(int categoryId);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(Category category);

    }
}
