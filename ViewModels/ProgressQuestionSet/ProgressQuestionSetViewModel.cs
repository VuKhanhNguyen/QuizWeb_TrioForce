using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.ProgressQuestionSet
{
    public class ProgressQuestionSetViewModel
    {
        [Required]
        public int QSetId { get; set; }

        [Required]
        public int QuestionCount { get; set; }

        [Required]
        public int QuestionLastId { get; set; }
    }
}
