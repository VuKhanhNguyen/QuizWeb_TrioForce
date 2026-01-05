namespace QuizWeb_TrioForce.ViewModels.Duel
{
    /// <summary>
    /// DTO cho dữ liệu câu hỏi gửi đến client
    /// </summary>
    public class QuestionDataDto
    {
        public int QuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public List<AnswerOptionDto> Answers { get; set; } = new();
        public int TimeLimit { get; set; } = 10; // seconds
    }

    public class AnswerOptionDto
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; } = null!;
    }

    /// <summary>
    /// DTO cho kết quả trả lời câu hỏi
    /// </summary>
    public class AnswerResultDto
    {
        public bool IsCorrect { get; set; }
        public int ScoreEarned { get; set; }
        public int TotalScore { get; set; }
        public bool BothPlayersAnswered { get; set; }
        public int CorrectAnswerId { get; set; }
        public int Player1TotalScore { get; set; }
        public int Player2TotalScore { get; set; }
        public bool IsMatchOver { get; set; }
    }

    /// <summary>
    /// DTO cho kết quả trận đấu
    /// </summary>
    public class MatchResultDto
    {
        public int MatchId { get; set; }
        public string? WinnerUserName { get; set; }
        public string? WinnerFullName { get; set; }
        public bool IsDraw { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public string Player1UserName { get; set; } = null!;
        public string? Player2UserName { get; set; }
        public string Player1FullName { get; set; } = null!;
        public string? Player2FullName { get; set; }
        public int Player1RankingChange { get; set; }
        public int Player2RankingChange { get; set; }
    }

    /// <summary>
    /// DTO cho kết quả reconnect
    /// </summary>
    public class ReconnectResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public MatchStateDto? MatchState { get; set; }
    }

    /// <summary>
    /// DTO cho trạng thái match hiện tại
    /// </summary>
    public class MatchStateDto
    {
        public int MatchId { get; set; }
        public string MatchCode { get; set; } = null!;
        public string Player1UserName { get; set; } = null!;
        public string? Player2UserName { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
        public string Status { get; set; } = null!;
        public QuestionDataDto? CurrentQuestion { get; set; }
    }

    /// <summary>
    /// ViewModel cho trang Index (tạo/join phòng)
    /// </summary>
    public class DuelIndexViewModel
    {
        public List<QuestionSetOptionDto> QuestionSets { get; set; } = new();
        public DuelRankingViewModel? UserRanking { get; set; }
    }

    public class QuestionSetOptionDto
    {
        public int QSetId { get; set; }
        public string QSetName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string LevelName { get; set; } = null!;
        public int QuestionCount { get; set; }
    }

    /// <summary>
    /// ViewModel cho bảng xếp hạng
    /// </summary>
    public class DuelRankingViewModel
    {
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int TotalMatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int TotalScore { get; set; }
        public double WinRate { get; set; }
        public int Rank { get; set; }
    }

    /// <summary>
    /// ViewModel cho lịch sử match
    /// </summary>
    public class MatchHistoryViewModel
    {
        public int MatchId { get; set; }
        public string OpponentUserName { get; set; } = null!;
        public string OpponentFullName { get; set; } = null!;
        public int MyScore { get; set; }
        public int OpponentScore { get; set; }
        public string Result { get; set; } = null!; // "Win", "Lose", "Draw"
        public DateTime PlayedAt { get; set; }
        public string QuestionSetName { get; set; } = null!;
    }
}
