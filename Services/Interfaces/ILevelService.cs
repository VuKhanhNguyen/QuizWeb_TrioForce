using quizweb.Models;

namespace quizweb.Services.Interfaces
{
    public interface ILevelService
    {
        public Task AddLevelAsync(Level level);
        public Task UpdateLevelAsync(Level level);
        public Task DeleteLevelAsync(int id);
        public Task<List<Level>> GetAllLevelsAsync();
        public Task<Level> GetLevelByIdAsync(int id);

    }
}
