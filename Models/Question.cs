using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int QSetId { get; set; }

        //navigation property
        public QuestionSet QuestionSet { get; set; } = null!;
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
