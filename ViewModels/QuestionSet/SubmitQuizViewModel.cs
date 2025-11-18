using QuizWeb_TrioForce.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.QuestionSet
{
    public class SubmitQuizViewModel
    {
        [Required(ErrorMessage = "Question Set ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Question Set ID")]
        public int QSetId { get; set; }

        [Required(ErrorMessage = "Question Set Name is required")]
        public string QSetName { get; set; } = null!;

        [Required(ErrorMessage = "User answers are required")]
        [MinLength(1, ErrorMessage = "At least one answer is required")]
        public List<UserAnswerViewModel> UserAnswers { get; set; } = [];

        [Required(ErrorMessage = "Question Count is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Question Count")]
        public int QuestionCount { get; set; }

        [Required(ErrorMessage = "Question Last Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Question Last ID")]
        public int QuestionLastId { get; set; }

    }
}
