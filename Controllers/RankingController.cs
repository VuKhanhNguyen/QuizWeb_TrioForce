using Microsoft.AspNetCore.Mvc;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels;

namespace QuizWeb_TrioForce.Controllers
{
    public class RankingController : Controller
    {
        private readonly ILogger<RankingController> _logger;
        private readonly IRankingService _rankingService;

        public RankingController(ILogger<RankingController> logger, IRankingService rankingService)
        {
            _logger = logger;
            _rankingService = rankingService;
        }

        // GET: RankingController
        public async Task<IActionResult> Index()
        {
            var rankings = await _rankingService.GetTopRankingsAsync(10);
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
    }
}
