namespace QuizWeb_TrioForce.Models
{
    public class DuelAnswer
    {
        public int DuelAnswerId { get; set; }
        public int MatchId { get; set; }
        public string UserName { get; set; } = null!;
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }

        public bool IsCorrect { get; set; }
        public double ResponseTimeSeconds { get; set; }  // Thời gian trả lời (giây)
        public int ScoreEarned { get; set; }             // Điểm nhận được
        public DateTime AnsweredAt { get; set; } = DateTime.Now;

        // Navigation properties
        public DuelMatch Match { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public Answer Answer { get; set; } = null!;
    }
}
