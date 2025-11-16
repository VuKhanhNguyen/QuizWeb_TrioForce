using System.ComponentModel.DataAnnotations;

namespace quizweb.Models
{
    public class Level
    {
        public int LevelId { get; set; }

        [StringLength(50, ErrorMessage = "Level name cannot be longer than 50 characters.")]
        public string LevelName { get; set; } = null!;

        public ICollection<QuestionSet> QuestionSets { get; set; } = new List<QuestionSet>();
    }
}
