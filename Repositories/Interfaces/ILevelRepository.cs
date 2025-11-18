using quizweb.Models;

namespace quizweb.Repositories.Interfaces
{
    public interface ILevelRepository
    {
        Task<List<Level>> GetAllLevelsAsync();
        Task<Level?> GetLevelByIdAsync(int id);
        Task AddLevelAsync(Level level);
        Task UpdateLevelAsync(Level level);
        Task DeleteLevelAsync(Level level);
    }
}
