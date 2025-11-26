using QuizWeb_TrioForce.Models;

namespace QuizWeb_TrioForce.Repositories.Interfaces
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
