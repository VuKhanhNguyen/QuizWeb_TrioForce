namespace QuizWeb_TrioForce.Models
{
    public class DuelRanking
    {
        public string UserName { get; set; } = null!;
        public int TotalMatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int TotalScore { get; set; }

        // Calculated property
        public double WinRate => TotalMatches > 0 ? (double)Wins / TotalMatches * 100 : 0;

        // Navigation property
        public ApplicationUser User { get; set; } = null!;
    }
}
