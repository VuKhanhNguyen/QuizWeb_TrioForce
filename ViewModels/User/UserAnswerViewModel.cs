using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.User
{
    public class UserAnswerViewModel
    {
        [Required(ErrorMessage = "Question ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Question ID")]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Answer selection is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Answer ID")]
        public int SelectedAnswerId { get; set; }
    }
}
