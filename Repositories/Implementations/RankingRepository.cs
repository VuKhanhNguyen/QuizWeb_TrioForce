using Microsoft.EntityFrameworkCore;
using QuizWeb_TrioForce.Data;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;

namespace QuizWeb_TrioForce.Repositories.Implementations
{
    public class RankingRepository : IRankingRepository
    {
        private readonly AppDbContext _context;

        public RankingRepository(AppDbContext context)
        {
             _context = context;
        }

        public async Task CreateInitialRankingAsync(string username)
        {
            await _context.Rankings.AddAsync(new Ranking
            {
                UserName = username,
                TotalScore = 0
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<Ranking>> GetTopRankingsAsync(int topN)
        {
            return await _context.Rankings.AsNoTracking().OrderByDescending(r => r.TotalScore).Take(topN).ToListAsync();
        }

        public async Task<Ranking?> GetUserRankingAsync(string username)
        {
            return await _context.Rankings.FindAsync(username);
        }

        public async Task UpdateUserScoreAsync(string username, int score)
        {
            var user = await _context.Rankings.FindAsync(username);
            if (user != null)
            {
                user.TotalScore += score;
                _context.Rankings.Update(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
