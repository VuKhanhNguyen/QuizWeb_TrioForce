using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.ViewModels.Question;

namespace QuizWeb_TrioForce.ViewModels.QuestionSet
{
    public class PlayQuestionSetViewModel
    {
        public int QSetId { get; set; }

        public string QSetName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string AuthorName { get; set; } = null!;

        public string LevelName { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public List<PlayQuestionViewModel> Questions { get; set; } = [];
    }
}
