using QuizWeb_TrioForce.ViewModels.Answer;
using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.Question
{
    public class CreateQuestionViewModel
    {
        [Required(ErrorMessage ="Question Text is required")]
        [StringLength(1000,MinimumLength = 10,  ErrorMessage = "Question Text must be betwween 10 and 1000 characters")]
        [Display(Name ="Question")]
        public string QuestionText { get; set; } = null!;

        [MinLength(2, ErrorMessage ="Each question must have at least 2 answers")]
        [MaxLength(6, ErrorMessage =" Each question can have maximum 6 answers")]
        public List<CreateAnswerViewModel> Answers { get; set; } = new();
    }
}
