namespace quizweb.Models
{
    public class MarkedQuestion
    {
        public string UserName { get; set; } = null!;
        public int QuestionId { get; set; }
        public DateTime MarkedTime { get; set; } = DateTime.Now;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Question Question { get; set; } = null!;
    }
}
