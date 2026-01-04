using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.ViewModels.Duel;

namespace QuizWeb_TrioForce.Repositories.Interfaces
{
    public interface IDuelRepository
    {
        // Matches
        Task AddMatchAsync(DuelMatch match);
        Task<DuelMatch?> FindMatchAsync(int matchId); // basic Find (no includes)
        Task<DuelMatch?> GetMatchForLobbyByIdAsync(int matchId);
        Task<DuelMatch?> GetMatchForLobbyByCodeAsync(string matchCode);
        Task<DuelMatch?> GetWaitingMatchForJoinAsync(string matchCode);
        Task<DuelMatch?> GetMatchWithQuestionsAsync(int matchId); // include Questions + Answers

        // Answers
        Task AddAnswerAsync(DuelAnswer duelAnswer);

        // Rankings
        Task<DuelRanking> GetOrCreateRankingAsync(string userName);
        Task<List<DuelRanking>> GetTopRankingsAsync(int take = 50);

        // History / active matches
        Task<List<DuelMatch>> GetUserMatchHistoryAsync(string userName, int take = 10);
        Task<List<DuelMatch>> GetActiveMatchesForUserAsync(string userName);
        Task<List<DuelMatch>> GetInProgressMatchesForUserAsync(string userName);

        // QuestionSet helpers
        Task<int> GetRandomQuestionSetIdAsync();
        Task<List<QuestionSetOptionDto>> GetAllQuestionSetsForSelectAsync();

        // Unit of work
        Task SaveChangesAsync();
    }
}