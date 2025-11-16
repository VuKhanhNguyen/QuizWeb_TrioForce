using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace quizweb.Models
{
    public class QuestionSet
    {
        public int QSetId { get; set; }

        public string QSetName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string AuthorName { get; set; } = null!;

        public int LevelId { get; set; }

        public int CategoryId { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.Now;

        // Navigation properties
        public ApplicationUser Author { get; set; } = null!;
        public Level Level { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<ProgressQuestionSet> ProgressQuestionSets { get; set; } = new List<ProgressQuestionSet>();
        public ICollection<AnsweredQuestion> AnsweredQuestions { get; set; } = new List<AnsweredQuestion>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();

    }
}
