using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace quizweb.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100, ErrorMessage = "Full name cannot be longer than 100 characters.")]
        public string FullName { get; set; } = null!;

        [Required]
        public DateOnly BirthDay { get; set; }

        public bool Sex { get; set; }

        // Navigation properties
        public ICollection<QuestionSet> QuestionSets { get; set; } = new List<QuestionSet>();
        public ICollection<AnsweredQuestion> AnsweredQuestions { get; set; } = new List<AnsweredQuestion>();
        public ICollection<ProgressQuestionSet> ProgressQuestionSets { get; set; } = new List<ProgressQuestionSet>();
        public ICollection<MarkedQuestion> MarkedQuestions { get; set; } = new List<MarkedQuestion>();
        public Ranking Ranking { get; set; } = null!;


    }
}
