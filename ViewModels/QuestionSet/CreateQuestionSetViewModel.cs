using Microsoft.AspNetCore.Mvc.Rendering;
using QuizWeb_TrioForce.ViewModels.Question;
using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.QuestionSet
{
    public class CreateQuestionSetViewModel
    {
        //public int QSetId { get; set; } Khong can, vi luc convert sang model thi QSetId se tu sinh ra

        [Required(ErrorMessage ="Quiz name is required")]
        [StringLength(200, MinimumLength =5, ErrorMessage ="Quiz name must be between 5 and 200 characters")]
        [Display(Name= "Quiz name")]
        public string QSetName { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage ="Please select difficulty level")]
        [Display(Name ="Difficulty level")]
        public int LevelId { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public List<CreateQuestionViewModel> Questions { get; set; } = [];

        public List<SelectListItem> Levels { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();   
    }
}
