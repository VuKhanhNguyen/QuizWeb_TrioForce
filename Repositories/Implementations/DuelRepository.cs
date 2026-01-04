using Microsoft.EntityFrameworkCore;
using QuizWeb_TrioForce.Data;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;
using QuizWeb_TrioForce.ViewModels.Duel;

namespace QuizWeb_TrioForce.Repositories.Implementations
{
    public class DuelRepository : IDuelRepository
    {
        private readonly AppDbContext _context;

        public DuelRepository(AppDbContext context)
        {
            _context = context;
        }

        // ===================== Matches =====================

        public Task AddMatchAsync(DuelMatch match)
        {
            _context.DuelMatches.Add(match);
            return Task.CompletedTask;
        }

        public async Task<DuelMatch?> FindMatchAsync(int matchId)
        {
            return await _context.DuelMatches.FindAsync(matchId);
        }

        public async Task<DuelMatch?> GetMatchForLobbyByIdAsync(int matchId)
        {
            return await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.QuestionSet)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);
        }

        public async Task<DuelMatch?> GetMatchForLobbyByCodeAsync(string matchCode)
        {
            return await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.QuestionSet)
                .FirstOrDefaultAsync(m => m.MatchCode == matchCode);
        }

        public async Task<DuelMatch?> GetWaitingMatchForJoinAsync(string matchCode)
        {
            return await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.QuestionSet)
                .FirstOrDefaultAsync(m => m.MatchCode == matchCode && m.Status == MatchStatus.Waiting);
        }

        public async Task<DuelMatch?> GetMatchWithQuestionsAsync(int matchId)
        {
            return await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.QuestionSet)
                    .ThenInclude(qs => qs.Questions)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);
        }

        // ===================== Answers =====================

        public Task AddAnswerAsync(DuelAnswer duelAnswer)
        {
            _context.DuelAnswers.Add(duelAnswer);
            return Task.CompletedTask;
        }

        // ===================== Rankings =====================

        public async Task<DuelRanking> GetOrCreateRankingAsync(string userName)
        {
            var ranking = await _context.DuelRankings.FindAsync(userName);
            if (ranking == null)
            {
                ranking = new DuelRanking { UserName = userName };
                _context.DuelRankings.Add(ranking);
                await _context.SaveChangesAsync();
            }
            return ranking;
        }

        public async Task<List<DuelRanking>> GetTopRankingsAsync(int take = 50)
        {
            return await _context.DuelRankings
                .AsNoTracking()
                .Include(r => r.User)
                .OrderByDescending(r => r.TotalScore)
                .ThenByDescending(r => r.Wins)
                .Take(take)
                .ToListAsync();
        }

        // ===================== History / Active =====================

        public async Task<List<DuelMatch>> GetUserMatchHistoryAsync(string userName, int take = 10)
        {
            return await _context.DuelMatches
                .AsNoTracking()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.QuestionSet)
                .Where(m => (m.Player1UserName == userName || m.Player2UserName == userName)
                         && m.Status == MatchStatus.Completed)
                .OrderByDescending(m => m.EndedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<DuelMatch>> GetActiveMatchesForUserAsync(string userName)
        {
            return await _context.DuelMatches
                .AsNoTracking()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.QuestionSet)
                .Where(m => (m.Player1UserName == userName || m.Player2UserName == userName)
                         && m.Status == MatchStatus.InProgress)
                .ToListAsync();
        }

        public async Task<List<DuelMatch>> GetInProgressMatchesForUserAsync(string userName)
        {
            // giống Active, nhưng để rõ intent khi gọi từ service
            return await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => (m.Player1UserName == userName || m.Player2UserName == userName)
                         && m.Status == MatchStatus.InProgress)
                .ToListAsync();
        }

        // ===================== QuestionSet helpers =====================

        public async Task<int> GetRandomQuestionSetIdAsync()
        {
            var ids = await _context.QuestionSets
                .Where(qs => qs.Questions.Count > 0)
                .Select(qs => qs.QSetId)
                .ToListAsync();

            if (ids.Count == 0)
                throw new InvalidOperationException("Không có bộ câu hỏi nào.");

            return ids[Random.Shared.Next(ids.Count)];
        }

        public async Task<List<QuestionSetOptionDto>> GetAllQuestionSetsForSelectAsync()
        {
            return await _context.QuestionSets
                .AsNoTracking()
                .Include(qs => qs.Category)
                .Include(qs => qs.Level)
                .Include(qs => qs.Questions)
                .Where(qs => qs.Questions.Count > 0)
                .Select(qs => new QuestionSetOptionDto
                {
                    QSetId = qs.QSetId,
                    QSetName = qs.QSetName,
                    CategoryName = qs.Category.CategoryName,
                    LevelName = qs.Level.LevelName,
                    QuestionCount = qs.Questions.Count
                })
                .ToListAsync();
        }

        // ===================== Unit of work =====================

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}
