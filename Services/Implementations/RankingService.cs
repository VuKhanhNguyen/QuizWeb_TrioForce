using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;
using QuizWeb_TrioForce.Services.Implementations;
using QuizWeb_TrioForce.Services.Interfaces;

namespace QuizWeb_TrioForce.Services.Implementations
{
    public class RankingService : IRankingService
    {
        private readonly IRankingRepository _rankingRepository;

        public RankingService(IRankingRepository rankingRepository)
        {
            _rankingRepository = rankingRepository;
        }

        public async Task CreateInitialRankingAsync(string username)
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            await _rankingRepository.CreateInitialRankingAsync(username);
        }

        public async Task<List<Ranking>> GetTopRankingsAsync(int topN)
        {
            return await _rankingRepository.GetTopRankingsAsync(topN);
        }

        public async Task<Ranking> GetUserRankingAsync(string username)
        {
            var ranking = await _rankingRepository.GetUserRankingAsync(username);

            return ranking ?? throw new Exception("UserRanking is not found");
        }

        public async Task UpdateUserScoreAsync(string username, int score)
        {
            await _rankingRepository.UpdateUserScoreAsync(username, score);
        }
    }
}
