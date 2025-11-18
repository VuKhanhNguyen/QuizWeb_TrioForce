using QuizWeb_TrioForce.ViewModels.Answer;

namespace QuizWeb_TrioForce.ViewModels.Question
{
    public class PlayQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public List<PlayAnswerViewModel> Answers { get; set; } = [];
        //public int? SelectAnswerId { get; set; }
    }
}