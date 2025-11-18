using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.Category
{
    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage ="Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot be longer than 100 characters.")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "Image file is required")]
        public IFormFile ImageFile { get; set; } = null!;
    }
}
