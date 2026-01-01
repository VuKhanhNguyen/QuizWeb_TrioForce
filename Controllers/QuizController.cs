using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuizWeb_TrioForce.Services.Implementations;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.ProgressQuestionSet;
using QuizWeb_TrioForce.ViewModels.QuestionSet;
using System.Text.Json;

namespace QuizWeb_TrioForce.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly ILogger<QuizController> _logger;
        private readonly IQuizService _quizService;
        private readonly ILevelService _levelService;
        private readonly ICategoryService _categoryService;
        private readonly IMarkedQuestionService _markedQuestionService;

        public QuizController(ILogger<QuizController> logger, IQuizService quizService, ILevelService levelService, ICategoryService categoryService, IMarkedQuestionService markedQuestionService)
        {
            _logger = logger;
            _quizService = quizService;
            _levelService = levelService;
            _categoryService = categoryService;
            _markedQuestionService = markedQuestionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateQuestionSetViewModel();

            await GetAddSelectItemList(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuestionSetViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var username = User.Identity?.Name;
                    if (username == null)
                    {
                        return NotFound();
                    }
                    await _quizService.CreateQuizAsync(viewModel, username);
                    return RedirectToAction("Index", "Quiz");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to create quiz: " + ex.Message);
                }
            }

            await GetAddSelectItemList(viewModel);
            return View(viewModel);
        }


        private async Task GetAddSelectItemList(CreateQuestionSetViewModel viewModel)
        {
            var levelList = await _levelService.GetAllLevelsAsync();
            var cateList = await _categoryService.GetAllCategoryAsync();

            viewModel.Levels = levelList.Select(l => new SelectListItem
            {
                Text = l.LevelName,
                Value = l.LevelId.ToString()
            }).ToList();

            viewModel.Categories = cateList.Select(c => new SelectListItem
            {
                Text = c.CategoryName,
                Value = c.CategoryId.ToString()
            }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Play(int? QSetId)
        {
            var username = User.Identity?.Name;
            if (username == null)
            {
                return NotFound();
            }

            PlayQuestionSetViewModel viewModel;
            if (QSetId.HasValue)
            {
                viewModel = await _quizService.GetQuizAsync(QSetId.Value);
            }
            else
            {
                viewModel = await _quizService.GetRandomQuizAsync();
            }
            var markedQuestions = await _markedQuestionService.GetAllMarkedQuestionsByQSetIdAsync(username, viewModel.QSetId);

            viewModel.Questions.ForEach(q => q.IsMarked = markedQuestions.Any(mq => mq.QuestionId == q.QuestionId));
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Play(SubmitQuizViewModel submitQuiz)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var username = User.Identity?.Name;
                    if (username == null)
                    {
                        return NotFound();
                    }
                    var quizResultViewModel = await _quizService.SubmitQuizAsync(submitQuiz, username);
                    if (quizResultViewModel == null)
                    {
                        return NotFound();
                    }
                    //return RedirectToAction("Result", quizResultViewModel); => Redirection with complex objects: http302, objects serialized to query string, lost nested objects
                    TempData["QuizResult"] = JsonSerializer.Serialize(quizResultViewModel);
                    return RedirectToAction("Result");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting quiz");
                    ModelState.AddModelError("", "An error occurred while submitting the quiz.");
                    return RedirectToAction("Play", new { qSetId = submitQuiz.QSetId });
                }
            }
            return RedirectToAction("Play", new { qSetId = submitQuiz.QSetId });
        }

        public IActionResult Result()
        {
            if (TempData["QuizResult"] is string json)
            {
                var viewModel = JsonSerializer.Deserialize<QuizResultViewModel>(json);
                return View(viewModel);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgress(SaveProgressViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var username = User.Identity?.Name;
            if (username == null)
            {
                return Unauthorized();
            }

            await _quizService.SaveProgressAsync(viewModel, username);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> ResumeProgress(int QSetId)
        {
            var username = User.Identity?.Name;
            if (username == null)
            {
                return Unauthorized();
            }
            var progress = await _quizService.LoadProgressAsync(username, QSetId);
            if (progress == null)
            {
                return NotFound();
            }

            var markedQuestions = await _markedQuestionService.GetAllMarkedQuestionsByQSetIdAsync(username, QSetId);

            progress.Questions.ForEach(q => q.IsMarked = markedQuestions.Any(mq => mq.QuestionId == q.QuestionId));


            return View("Play", progress);
        }

        // [HttpGet]
        // public async Task<IActionResult> Play(int categoryId, int levelId)
        // {
        //     try
        //     {
        //         var viewModel = await _quizService.GetQuizByCategoryAndLevelAsync(categoryId, levelId);
        //         return View(viewModel);
        //     }
        //     catch (Exception ex)
        //     {
        //         // Log the exception
        //         _logger.LogError(ex, "Could not get quiz for category {categoryId} and level {levelId}", categoryId, levelId);
        //         // Maybe show a friendly error page or redirect with an error message
        //         TempData["ErrorMessage"] = "Không tìm thấy bộ câu hỏi phù hợp. Vui lòng thử lại sau.";
        //         return RedirectToAction("Index", "Home");
        //     }
        // }
    }
}
