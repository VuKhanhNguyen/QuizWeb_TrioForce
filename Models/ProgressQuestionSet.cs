namespace quizweb.Models
{
    public class ProgressQuestionSet
    {

        public string UserName { get; set; } = null!;
        public int QSetId { get; set; }
        public int QuestionCount { get; set; }
        public int QuestionLastId { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public QuestionSet QuestionSet { get; set; } = null!;
        public Question Question { get; set; } = null!; // Assuming you want to track the last question answered
    }
}
    