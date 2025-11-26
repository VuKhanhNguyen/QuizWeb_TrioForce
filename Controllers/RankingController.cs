using Microsoft.AspNetCore.Mvc;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels;

namespace QuizWeb_TrioForce.Controllers
{
    public class RankingController : Controller
    {
        private readonly ILogger<RankingController> _logger;
        private readonly IRankingService _rankingService;
        private readonly IUserService _userService;

        public RankingController(ILogger<RankingController> logger, IRankingService rankingService, IUserService userService)
        {
            _logger = logger;
            _rankingService = rankingService;
            _userService = userService;
        }

        // GET: RankingController
        public async Task<IActionResult> Index()
        {
            var rankings = await _rankingService.GetTopRankingsAsync(int.MaxValue);
            var viewModels = rankings.Select(r => new RankingListViewModel
            {
                Username = r.UserName,
                TotalScore = r.TotalScore
            }).ToList();
            return View(viewModels);
        }

        // GET: RankingController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        // GET: RankingController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: RankingController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: RankingController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        // POST: RankingController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: RankingController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        // POST: RankingController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: API endpoint to get player stats
        [HttpGet]
        public async Task<IActionResult> GetPlayerStats(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { success = false, message = "Username is required" });
                }

                var user = await _userService.GetProfileAsync(username);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var rankings = await _rankingService.GetTopRankingsAsync(int.MaxValue);
                var userRanking = rankings.FirstOrDefault(r => r.UserName == username);
                var rank = rankings.ToList().FindIndex(r => r.UserName == username) + 1;

                var totalGames = await _userService.GetTotalGamesPlayedAsync(username);
                var (totalAnswered, correctAnswers) = await _userService.GetAnswerStatsAsync(username);
                var accuracyRate = totalAnswered > 0 ? Math.Round((double)correctAnswers / totalAnswered * 100, 2) : 0;
                var questionSetsCreated = (await _userService.GetCreatedQuestionSetsAsync(username)).Count;

                var stats = new PlayerStatsViewModel
                {
                    Username = user.UserName ?? "",
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    TotalScore = userRanking?.TotalScore ?? 0,
                    Rank = rank,
                    TotalGamesPlayed = totalGames,
                    TotalQuestionsAnswered = totalAnswered,
                    CorrectAnswers = correctAnswers,
                    AccuracyRate = accuracyRate,
                    QuestionSetsCreated = questionSetsCreated
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting player stats for {Username}", username);
                return Json(new { success = false, message = "An error occurred" });
            }
        }
    }
}
