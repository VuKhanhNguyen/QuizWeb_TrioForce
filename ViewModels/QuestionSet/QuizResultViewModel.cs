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
        public int UserSelecteAnswerId { get; set; }
        public int CorrectAnswerId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
