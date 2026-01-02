using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizWeb_TrioForce.Services.Implementations;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.BookMark;

namespace QuizWeb_TrioForce.Controllers
{
    [Authorize]
    public class BookmarkController : Controller
    {
        private readonly IMarkedQuestionService _markedQuestionService;
        private readonly IProgressQuestionSetService _progressQuestionSetService;
        private readonly IQuestionSetService _questionSetService;

        public BookmarkController(IMarkedQuestionService markedQuestionService, IProgressQuestionSetService progressQuestionSetService, IQuestionSetService questionSetService)
        {
            _markedQuestionService = markedQuestionService;
            _progressQuestionSetService = progressQuestionSetService;
            _questionSetService = questionSetService;
        }
        
        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var progressList = await _progressQuestionSetService.GetAllProgressQuestionSets(username);
            var markedList = await _markedQuestionService.GetAllMarkedQuestionsAsync(username);
            var createdList = await _questionSetService.GetAllCreatedQuestionSetsAsync(username);

            var bookmarkViewModel = new BookMarkViewModel()
            {
                ProgressQuestionSetList = progressList,
                CreatedQuestionList = createdList,
                MarkedQuestionList = markedList
            };

            return View(bookmarkViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveQuestion([FromBody] int questionId)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            await _markedQuestionService.AddMarkedQuestion(username, questionId);

            return NoContent();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsaveQuestion([FromBody] int questionId)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            await _markedQuestionService.RemoveMarkedQuestion(username, questionId);

            return NoContent();
        }
    }
}
