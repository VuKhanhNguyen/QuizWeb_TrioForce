using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.ViewModels.Duel;

namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IDuelService
    {
        /// <summary>
        /// Tạo phòng đấu mới
        /// </summary>
        Task<DuelMatch> CreateMatchAsync(string userName, int qSetId);

        /// <summary>
        /// Tham gia phòng đấu bằng mã
        /// </summary>
        Task<DuelMatch?> JoinMatchAsync(string matchCode, string userName);

        /// <summary>
        /// Lấy match theo ID
        /// </summary>
        Task<DuelMatch?> GetMatchByIdAsync(int matchId);

        /// <summary>
        /// Lấy match theo mã phòng
        /// </summary>
        Task<DuelMatch?> GetMatchByCodeAsync(string matchCode);

        /// <summary>
        /// Bắt đầu trận đấu
        /// </summary>
        Task<DuelMatch?> StartMatchAsync(int matchId, string userName);

        /// <summary>
        /// Lấy câu hỏi hiện tại của match
        /// </summary>
        Task<QuestionDataDto?> GetCurrentQuestionAsync(int matchId);

        /// <summary>
        /// Gửi câu trả lời
        /// </summary>
        Task<AnswerResultDto> SubmitAnswerAsync(int matchId, string userName, int questionId, int answerId, double responseTime);

        /// <summary>
        /// Kết thúc trận đấu
        /// </summary>
        Task<MatchResultDto> EndMatchAsync(int matchId);

        /// <summary>
        /// Xử lý khi player disconnect
        /// </summary>
        Task HandlePlayerDisconnectAsync(string userName);

        /// <summary>
        /// Xử lý khi player reconnect
        /// </summary>
        Task<ReconnectResultDto> HandlePlayerReconnectAsync(int matchId, string userName);

        /// <summary>
        /// Xử lý khi player rời phòng
        /// </summary>
        Task HandlePlayerLeaveAsync(int matchId, string userName);

        /// <summary>
        /// Lấy lịch sử các trận đấu của user
        /// </summary>
        Task<List<DuelMatch>> GetUserMatchHistoryAsync(string userName, int take = 10);

        /// <summary>
        /// Lấy bảng xếp hạng 1v1
        /// </summary>
        Task<List<DuelRanking>> GetDuelRankingAsync(int take = 50);

        /// <summary>
        /// Lấy hoặc tạo DuelRanking cho user
        /// </summary>
        Task<DuelRanking> GetOrCreateDuelRankingAsync(string userName);

        /// <summary>
        /// Lấy bộ câu hỏi ngẫu nhiên
        /// </summary>
        Task<int> GetRandomQuestionSetIdAsync();

        /// <summary>
        /// Lấy tất cả bộ câu hỏi cho dropdown
        /// </summary>
        Task<List<QuestionSetOptionDto>> GetAllQuestionSetsForSelectAsync();

        /// <summary>
        /// Lấy các match đang active của user
        /// </summary>
        Task<List<DuelMatch>> GetActiveMatchesForUserAsync(string userName);

        /// <summary>
        /// Kết thúc match do disconnect timeout
        /// </summary>
        Task EndMatchDueToDisconnectAsync(int matchId, string disconnectedPlayerUserName);
    }
}
