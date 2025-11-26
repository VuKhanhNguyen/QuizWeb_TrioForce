using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels
{
    public class UserEditViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = null!;

        [Required]
        [Display(Name = "Full Name")]
        public string Fullname { get; set; } = null!;

        [Display(Name = "Birthday")]
        public string Birthday { get; set; } = null!;

        [Display(Name = "Sex")]
        public bool Sex { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        // Thống kê bổ sung
        public int TotalScore { get; set; }
        public int Rank { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int TotalQuestionsAnswered { get; set; }
        public int CorrectAnswers { get; set; }
        public double AccuracyRate { get; set; }
        public int QuestionSetsCreated { get; set; }
    }
}
