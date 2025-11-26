using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.Category;
using QuizWeb_TrioForce.ViewModels;
using QuizWeb_TrioForce.ViewModels.Level;

namespace QuizWeb_TrioForce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly ILevelService _levelService;

        public HomeController(ILogger<HomeController> logger, ICategoryService categoryService, ILevelService levelService)
        {
            _logger = logger;
            _categoryService = categoryService;
            _levelService = levelService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoryAsync();
            var categoryViewModels = categories.Select(c => new CategoryListViewModel()
            {
                CategoryId = c.CategoryId,
                Name = c.CategoryName,
                Url = c.ImgUrl
            }).ToList();

            var levels = await _levelService.GetAllLevelsAsync();
            var levelViewModels = levels.Select(l =>
            {
                string imageUrl = "";
                if (l.LevelName.ToLower() == "dễ")
                {
                    imageUrl = "/assets/level/easyyy.png";
                }
                else if (l.LevelName.ToLower() == "trung bình")
                {
                    imageUrl = "/assets/level/mediummm.png";
                }
                else if (l.LevelName.ToLower() == "khó")
                {
                    imageUrl = "/assets/level/harddd.png";
                }
                return new LevelViewModel
                {
                    LevelId = l.LevelId,
                    LevelName = l.LevelName,
                    ImageUrl = imageUrl
                };
            }).ToList();

            var viewModel = new HomeViewModel
            {
                Categories = categoryViewModels,
                Levels = levelViewModels
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
