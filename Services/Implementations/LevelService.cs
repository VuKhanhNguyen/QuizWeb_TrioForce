using quizweb.Models;
using quizweb.Repositories.Interfaces;
using quizweb.Services.Interfaces;

namespace quizweb.Services.Implementations
{
    public class LevelService : ILevelService
    {
        private readonly ILevelRepository _levelRepository;

        public LevelService(ILevelRepository levelRepository)
        {
            _levelRepository = levelRepository;
        }
        public async Task AddLevelAsync(Level level)
        {
            await _levelRepository.AddLevelAsync(level);
        }

        public async Task UpdateLevelAsync(Level level)
        {
            await _levelRepository.UpdateLevelAsync(level);
        }

        public async Task DeleteLevelAsync(int id)
        {
            var level = await _levelRepository.GetLevelByIdAsync(id);
            if (level != null)
            {
                await _levelRepository.DeleteLevelAsync(level);
            }
        }

        public async Task<List<Level>> GetAllLevelsAsync()
        {
            return await _levelRepository.GetAllLevelsAsync();
        }

        public async Task<Level> GetLevelByIdAsync(int id)
        {
            var level = await _levelRepository.GetLevelByIdAsync(id);
            if (level == null)
            {
                throw new Exception("Level not found");
            }
            return level;
        }

    }
}
