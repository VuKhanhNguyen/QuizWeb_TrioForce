using QuizWeb_TrioForce.ViewModels.Category;
using QuizWeb_TrioForce.ViewModels.Level;
using System.Collections.Generic;

namespace QuizWeb_TrioForce.ViewModels
{
    public class HomeViewModel
    {
        public List<CategoryListViewModel> Categories { get; set; } = new();
        public List<LevelViewModel> Levels { get; set; } = new();
    }
}
