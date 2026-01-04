using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.Answer
{
    public class UpdateAnswerViewModel
    {
        // AnswerId sẽ là null nếu là đáp án mới được thêm trong quá trình edit
        public int? AnswerId { get; set; }

        [Required(ErrorMessage = "Answer is required")]
        [StringLength(500, MinimumLength = 1,ErrorMessage = "Answer must be between 1 and 500 characters")]
        [Display(Name ="Answer")]
        public string AnswerText { get; set; } = null!;

        [Display(Name ="Is Correct Answer")]
        public bool IsCorrect{ get; set; }

    }
}
