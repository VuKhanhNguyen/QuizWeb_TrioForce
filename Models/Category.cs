using System.ComponentModel.DataAnnotations;

namespace quizweb.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [StringLength(100, ErrorMessage = "Category name cannot be longer than 50 characters.")]
        public string CategoryName { get; set; } = null!;

        [StringLength(200, ErrorMessage = "Image URL cannot be longer than 200 characters.")]
        public string ImgUrl { get; set; } = null!;

        public ICollection<QuestionSet> QuestionSets { get; set; } = new List<QuestionSet>();
    }
}
