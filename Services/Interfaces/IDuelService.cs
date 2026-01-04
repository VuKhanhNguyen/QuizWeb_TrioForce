using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.ViewModels.Duel;

namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IDuelService
    {
        Task<DuelMatch> CreateMatchAsync(string userName, int qSetId);
        Task<DuelMatch?> JoinMatchAsync(string matchCode, string userName);

        Task<DuelMatch?> GetMatchByIdAsync(int matchId);
        Task<DuelMatch?> GetMatchByCodeAsync(string matchCode);

        Task<DuelMatch?> StartMatchAsync(int matchId, string userName);
        Task<QuestionDataDto?> GetCurrentQuestionAsync(int matchId);
        Task<AnswerResultDto> SubmitAnswerAsync(int matchId, string userName, int questionId, int answerId, double responseTime);

        Task<MatchResultDto> EndMatchAsync(int matchId);

        Task HandlePlayerDisconnectAsync(string userName);
        Task<ReconnectResultDto> HandlePlayerReconnectAsync(int matchId, string userName);
        Task HandlePlayerLeaveAsync(int matchId, string userName);

        Task<List<DuelMatch>> GetUserMatchHistoryAsync(string userName, int take = 10);
        Task<List<DuelRanking>> GetDuelRankingAsync(int take = 50);

        Task<List<QuestionSetOptionDto>> GetAllQuestionSetsForSelectAsync();
        Task<List<DuelMatch>> GetActiveMatchesForUserAsync(string userName);

        Task EndMatchDueToDisconnectAsync(int matchId, string disconnectedPlayerUserName);

        Task<DuelRanking> GetOrCreateDuelRankingAsync(string userName);
    }
}
