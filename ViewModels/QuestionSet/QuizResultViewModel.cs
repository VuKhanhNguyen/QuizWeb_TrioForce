namespace QuizWeb_TrioForce.ViewModels.QuestionSet
{
    public class QuizResultViewModel
    {
        public int QSetId { get; set; }
        public string QSetName { get; set; } = null!;
        public int TotalQuestions { get; set; }
        public double Score { get; set; }
        public List<QuestionResultViewModel> QuestionResults { get; set; } = [];
    }

    public class QuestionResultViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int UserSelectedAnswerId { get; set; }
        public string UserSelectedAnswerText { get; set; } = string.Empty;
        public int CorrectAnswerId { get; set; }
        public string CorrectAnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public List<AnswerOptionViewModel> AllAnswers { get; set; } = [];
    }

    public class AnswerOptionViewModel
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public bool IsUserSelected { get; set; }
    }
}
