using QuizWeb_TrioForce.Models;


namespace QuizWeb_TrioForce.Repositories.Interfaces
{
    public interface IRankingRepository
    {
        Task<List<Ranking>> GetTopRankingsAsync(int topN);
        Task<Ranking?> GetUserRankingAsync(string username);
        Task CreateInitialRankingAsync(string username);
        Task UpdateUserScoreAsync(string username, int score);
    }
}