using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.Duel;

namespace QuizWeb_TrioForce.Controllers
{
    [Authorize]
    public class DuelController : Controller
    {
        private readonly IDuelService _duelService;
        private readonly ILogger<DuelController> _logger;

        public DuelController(
            IDuelService duelService, 
            ILogger<DuelController> logger)
        {
            _duelService = duelService;
            _logger = logger;
        }

        /// <summary>
        /// Trang chủ chế độ 1v1: Tạo phòng hoặc nhập mã
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            // Get available question sets
            var questionSets = await _duelService.GetAllQuestionSetsForSelectAsync();
            var userRanking = await _duelService.GetOrCreateDuelRankingAsync(userName);

            var viewModel = new DuelIndexViewModel
            {
                QuestionSets = questionSets,
                UserRanking = new DuelRankingViewModel
                {
                    UserName = userRanking.UserName,
                    TotalMatches = userRanking.TotalMatches,
                    Wins = userRanking.Wins,
                    Losses = userRanking.Losses,
                    Draws = userRanking.Draws,
                    TotalScore = userRanking.TotalScore,
                    WinRate = userRanking.WinRate
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// Phòng chờ - chờ đối thủ
        /// </summary>
        public async Task<IActionResult> Lobby(int matchId)
        {
            var match = await _duelService.GetMatchByIdAsync(matchId);
            if (match == null)
                return RedirectToAction(nameof(Index));

            var userName = User.Identity?.Name;
            if (match.Player1UserName != userName && match.Player2UserName != userName)
                return RedirectToAction(nameof(Index));

            return View(match);
        }

        /// <summary>
        /// Màn hình chơi
        /// </summary>
        public async Task<IActionResult> Play(int matchId)
        {
            var match = await _duelService.GetMatchByIdAsync(matchId);
            if (match == null)
                return RedirectToAction(nameof(Index));

            var userName = User.Identity?.Name;
            if (match.Player1UserName != userName && match.Player2UserName != userName)
                return RedirectToAction(nameof(Index));

            return View(match);
        }

        /// <summary>
        /// Kết quả trận đấu
        /// </summary>
        public async Task<IActionResult> Result(int matchId)
        {
            var match = await _duelService.GetMatchByIdAsync(matchId);
            if (match == null)
                return RedirectToAction(nameof(Index));

            var userName = User.Identity?.Name;
            if (match.Player1UserName != userName && match.Player2UserName != userName)
                return RedirectToAction(nameof(Index));

            // Build result view model
            var isPlayer1 = match.Player1UserName == userName;
            var myScore = isPlayer1 ? match.Player1Score : match.Player2Score;
            var opponentScore = isPlayer1 ? match.Player2Score : match.Player1Score;

            string result;
            if (match.WinnerUserName == userName)
                result = "Thắng";
            else if (match.WinnerUserName == null)
                result = "Hòa";
            else
                result = "Thua";

            ViewBag.MyScore = myScore;
            ViewBag.OpponentScore = opponentScore;
            ViewBag.Result = result;
            ViewBag.IsPlayer1 = isPlayer1;

            return View(match);
        }

        /// <summary>
        /// Lịch sử các trận đấu
        /// </summary>
        public async Task<IActionResult> History()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var matches = await _duelService.GetUserMatchHistoryAsync(userName, 20);
            
            var viewModel = matches.Select(m =>
            {
                var isPlayer1 = m.Player1UserName == userName;
                var myScore = isPlayer1 ? m.Player1Score : m.Player2Score;
                var opponentScore = isPlayer1 ? m.Player2Score : m.Player1Score;
                var opponentName = isPlayer1 ? m.Player2UserName : m.Player1UserName;
                var opponentFullName = isPlayer1 ? m.Player2?.FullName : m.Player1?.FullName;

                string result;
                if (m.WinnerUserName == userName)
                    result = "Win";
                else if (m.WinnerUserName == null)
                    result = "Draw";
                else
                    result = "Lose";

                return new MatchHistoryViewModel
                {
                    MatchId = m.MatchId,
                    OpponentUserName = opponentName ?? "N/A",
                    OpponentFullName = opponentFullName ?? opponentName ?? "N/A",
                    MyScore = myScore,
                    OpponentScore = opponentScore,
                    Result = result,
                    PlayedAt = m.EndedAt ?? m.CreatedAt,
                    QuestionSetName = m.QuestionSet?.QSetName ?? "N/A"
                };
            }).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// Bảng xếp hạng 1v1
        /// </summary>
        public async Task<IActionResult> Ranking()
        {
            var rankings = await _duelService.GetDuelRankingAsync(50);
            
            var viewModel = rankings.Select((r, index) => new DuelRankingViewModel
            {
                UserName = r.UserName,
                FullName = r.User?.FullName ?? r.UserName,
                TotalMatches = r.TotalMatches,
                Wins = r.Wins,
                Losses = r.Losses,
                Draws = r.Draws,
                TotalScore = r.TotalScore,
                WinRate = r.WinRate,
                Rank = index + 1
            }).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// API: Tạo phòng mới (fallback nếu không dùng SignalR)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoom(int qSetId = 0)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                    return Json(new { success = false, message = "Bạn cần đăng nhập." });

                var match = await _duelService.CreateMatchAsync(userName, qSetId);
                return Json(new { 
                    success = true, 
                    matchId = match.MatchId, 
                    matchCode = match.MatchCode 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Tham gia phòng bằng mã
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> JoinRoom(string matchCode)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                    return Json(new { success = false, message = "Bạn cần đăng nhập." });

                var match = await _duelService.JoinMatchAsync(matchCode.ToUpperInvariant(), userName);
                if (match == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng hoặc phòng đã đầy." });

                return Json(new { 
                    success = true, 
                    matchId = match.MatchId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
