namespace QuizWeb_TrioForce.Models
{
    public enum MatchStatus
    {
        Waiting,      // Đang chờ đối thủ
        InProgress,   // Đang diễn ra
        Completed,    // Hoàn thành
        Cancelled     // Hủy (hết thời gian chờ/disconnect)
    }

    public class DuelMatch
    {
        public int MatchId { get; set; }
        public string MatchCode { get; set; } = null!;  // Mã phòng (6 ký tự)
        public int QSetId { get; set; }                  // Bộ câu hỏi được chọn

        public string Player1UserName { get; set; } = null!;
        public string? Player2UserName { get; set; }     // Null khi đang chờ đối thủ

        public MatchStatus Status { get; set; } = MatchStatus.Waiting;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public int Player1Score { get; set; }
        public int Player2Score { get; set; }

        public string? WinnerUserName { get; set; }      // Null khi hòa/chưa xong

        public int CurrentQuestionIndex { get; set; }    // Câu hỏi hiện tại (0-indexed)

        // Reconnect handling
        public DateTime? Player1DisconnectedAt { get; set; }
        public DateTime? Player2DisconnectedAt { get; set; }

        // Navigation properties
        public ApplicationUser Player1 { get; set; } = null!;
        public ApplicationUser? Player2 { get; set; }
        public QuestionSet QuestionSet { get; set; } = null!;
        public ICollection<DuelAnswer> DuelAnswers { get; set; } = new List<DuelAnswer>();
    }
}
