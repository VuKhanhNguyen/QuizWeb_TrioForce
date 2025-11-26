namespace QuizWeb_TrioForce.ViewModels
{
    public class PlayerStatsViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalScore { get; set; }
        public int Rank { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int TotalQuestionsAnswered { get; set; }
        public int CorrectAnswers { get; set; }
        public double AccuracyRate { get; set; }
        public int QuestionSetsCreated { get; set; }
    }
}
