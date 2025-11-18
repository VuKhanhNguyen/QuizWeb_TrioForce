using Microsoft.EntityFrameworkCore;
using quizweb.Data;
using quizweb.Models;
using quizweb.Repositories.Interfaces;

namespace quizweb.Repositories.Implementations
{
    public class LevelRepository : ILevelRepository
    {
        private readonly AppDbContext _context;

        public LevelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddLevelAsync(Level level)
        {
            await _context.Levels.AddAsync(level);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLevelAsync(Level level)
        {
            _context.Levels.Update(level);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLevelAsync(Level level)
        {
                _context.Levels.Remove(level);
                await _context.SaveChangesAsync();
        }

        public async Task<List<Level>> GetAllLevelsAsync()
        {
            return await _context.Levels.AsNoTracking().ToListAsync();
        }

        public async Task<Level?> GetLevelByIdAsync(int id)
        {
            return await _context.Levels.FindAsync(id);
        }
        
    }
}
