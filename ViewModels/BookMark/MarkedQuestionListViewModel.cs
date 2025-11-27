namespace QuizWeb_TrioForce.ViewModels.BookMark
{
    public class MarkedQuestionListViewModel
    {
        public int QuestionId { get; init; }
        public string QuestionText { get; init; } = null!;
        public string AnswerTrue { get; init; } = null!;
        public DateTime MarkedTime { get; init; }

    }
}
