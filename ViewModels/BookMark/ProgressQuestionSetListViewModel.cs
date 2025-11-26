using QuizWeb_TrioForce.ViewModels.ProgressQuestionSet;

namespace QuizWeb_TrioForce.ViewModels.BookMark
{
    public class ProgressQuestionSetListViewModel : ProgressQuestionSetViewModel
    {
        public string QSetName { get; set; } = null!;
        public int TotalQuestions { get; set; }
        public string AuthorName { get; set; } = null!;
        public DateTime LastUpdated { get; set; }

    }
}
