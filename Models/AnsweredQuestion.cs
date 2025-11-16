using System.ComponentModel.DataAnnotations.Schema;

namespace quizweb.Models
{
    public class AnsweredQuestion
    {
        public string UserName { get; set; } = null!;
        public int QSetId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public QuestionSet QuestionSet { get; set; } = null!;
        public Answer Answer { get; set; } = null!;
    }
}
