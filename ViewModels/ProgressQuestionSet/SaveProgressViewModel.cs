using QuizWeb_TrioForce.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.ProgressQuestionSet
{
    public class SaveProgressViewModel : ProgressQuestionSetViewModel
    {
        public List<UserAnswerViewModel> UserAnswers { get; set; } = [];
    }
}
