using Microsoft.EntityFrameworkCore;
using QuizWeb_TrioForce.Data;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.Duel;

namespace QuizWeb_TrioForce.Services.Implementations
{
    public class DuelService : IDuelService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DuelService> _logger;
        private static readonly Random _random = new();

        // Track which players have answered current question (in production, use distributed cache)
        private static readonly Dictionary<int, HashSet<string>> MatchAnswers = new();

        public DuelService(AppDbContext context, ILogger<DuelService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DuelMatch> CreateMatchAsync(string userName, int qSetId)
        {
            // Nếu qSetId = 0, chọn ngẫu nhiên
            if (qSetId <= 0)
            {
                qSetId = await GetRandomQuestionSetIdAsync();
            }

            var matchCode = GenerateMatchCode();
            
            var match = new DuelMatch
            {
                MatchCode = matchCode,
                QSetId = qSetId,
                Player1UserName = userName,
                Status = MatchStatus.Waiting,
                CreatedAt = DateTime.Now
            };

            _context.DuelMatches.Add(match);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await GetMatchByIdAsync(match.MatchId) ?? match;
        }

        public async Task<DuelMatch?> JoinMatchAsync(string matchCode, string userName)
        {
            var match = await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.QuestionSet)
                .FirstOrDefaultAsync(m => m.MatchCode == matchCode && m.Status == MatchStatus.Waiting);

            if (match == null) return null;
            if (match.Player1UserName == userName) return null; // Can't join own room

            match.Player2UserName = userName;
            // Don't change status yet - wait for StartMatch

            await _context.SaveChangesAsync();

            // Reload with Player2
            return await GetMatchByIdAsync(match.MatchId);
        }

        public async Task<DuelMatch?> GetMatchByIdAsync(int matchId)
        {
            return await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.QuestionSet)
                    .ThenInclude(qs => qs.Questions)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);
        }

        public async Task<DuelMatch?> GetMatchByCodeAsync(string matchCode)
        {
            return await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.QuestionSet)
                .FirstOrDefaultAsync(m => m.MatchCode == matchCode);
        }

        public async Task<DuelMatch?> StartMatchAsync(int matchId, string userName)
        {
            var match = await GetMatchByIdAsync(matchId);
            if (match == null) return null;

            // Only Player1 can start
            if (match.Player1UserName != userName)
                throw new InvalidOperationException("Chỉ người tạo phòng mới có thể bắt đầu trận đấu.");

            // Must have Player2
            if (string.IsNullOrEmpty(match.Player2UserName))
                throw new InvalidOperationException("Cần có đối thủ để bắt đầu trận đấu.");

            match.Status = MatchStatus.InProgress;
            match.StartedAt = DateTime.Now;
            match.CurrentQuestionIndex = 0;

            // Initialize answer tracking
            MatchAnswers[matchId] = new HashSet<string>();

            await _context.SaveChangesAsync();
            return match;
        }

        public async Task<QuestionDataDto?> GetCurrentQuestionAsync(int matchId)
        {
            var match = await GetMatchByIdAsync(matchId);
            if (match == null || match.QuestionSet?.Questions == null) return null;

            var questions = match.QuestionSet.Questions.OrderBy(q => q.QuestionId).ToList();
            if (match.CurrentQuestionIndex >= questions.Count) return null;

            var question = questions[match.CurrentQuestionIndex];

            // Reset answer tracking for this question
            MatchAnswers[matchId] = new HashSet<string>();

            return new QuestionDataDto
            {
                QuestionIndex = match.CurrentQuestionIndex + 1,
                TotalQuestions = questions.Count,
                QuestionId = question.QuestionId,
                QuestionText = question.QuestionText,
                TimeLimit = 10,
                Answers = question.Answers.Select(a => new AnswerOptionDto
                {
                    AnswerId = a.AnswerId,
                    AnswerText = a.AnswerText
                }).OrderBy(_ => _random.Next()).ToList() // Shuffle answers
            };
        }

        public async Task<AnswerResultDto> SubmitAnswerAsync(int matchId, string userName, int questionId, int answerId, double responseTime)
        {
            var match = await GetMatchByIdAsync(matchId);
            if (match == null)
                throw new InvalidOperationException("Không tìm thấy trận đấu.");

            // Check if answer is correct
            var question = match.QuestionSet?.Questions?.FirstOrDefault(q => q.QuestionId == questionId);
            var correctAnswer = question?.Answers?.FirstOrDefault(a => a.IsCorrect);
            bool isCorrect = answerId == correctAnswer?.AnswerId;

            // Calculate score: 15 base + bonus for time (max 5 bonus)
            int scoreEarned = 0;
            if (isCorrect && answerId > 0) // answerId = -1 means timeout
            {
                scoreEarned = 15 + Math.Max(0, (int)(5 - responseTime * 0.5));
            }

            // Save answer
            var duelAnswer = new DuelAnswer
            {
                MatchId = matchId,
                UserName = userName,
                QuestionId = questionId,
                SelectedAnswerId = answerId > 0 ? answerId : (correctAnswer?.AnswerId ?? answerId),
                IsCorrect = isCorrect,
                ResponseTimeSeconds = responseTime,
                ScoreEarned = scoreEarned,
                AnsweredAt = DateTime.Now
            };
            _context.DuelAnswers.Add(duelAnswer);

            // Update player score
            if (userName == match.Player1UserName)
                match.Player1Score += scoreEarned;
            else
                match.Player2Score += scoreEarned;

            await _context.SaveChangesAsync();

            // Track who answered
            if (!MatchAnswers.ContainsKey(matchId))
                MatchAnswers[matchId] = new HashSet<string>();
            MatchAnswers[matchId].Add(userName);

            bool bothAnswered = MatchAnswers[matchId].Count >= 2;
            
            // Check if match is over
            var questions = match.QuestionSet?.Questions?.OrderBy(q => q.QuestionId).ToList();
            bool isLastQuestion = match.CurrentQuestionIndex >= (questions?.Count ?? 0) - 1;

            // Move to next question if both answered
            if (bothAnswered && !isLastQuestion)
            {
                match.CurrentQuestionIndex++;
                await _context.SaveChangesAsync();
            }

            return new AnswerResultDto
            {
                IsCorrect = isCorrect,
                ScoreEarned = scoreEarned,
                TotalScore = userName == match.Player1UserName ? match.Player1Score : match.Player2Score,
                BothPlayersAnswered = bothAnswered,
                CorrectAnswerId = correctAnswer?.AnswerId ?? 0,
                Player1TotalScore = match.Player1Score,
                Player2TotalScore = match.Player2Score,
                IsMatchOver = bothAnswered && isLastQuestion
            };
        }

        public async Task<MatchResultDto> EndMatchAsync(int matchId)
        {
            var match = await GetMatchByIdAsync(matchId);
            if (match == null)
                throw new InvalidOperationException("Không tìm thấy trận đấu.");

            match.Status = MatchStatus.Completed;
            match.EndedAt = DateTime.Now;

            // Determine winner
            bool isDraw = match.Player1Score == match.Player2Score;
            string? winnerUserName = null;
            if (!isDraw)
            {
                winnerUserName = match.Player1Score > match.Player2Score 
                    ? match.Player1UserName 
                    : match.Player2UserName;
            }
            match.WinnerUserName = winnerUserName;

            await _context.SaveChangesAsync();

            // Update rankings
            var player1RankingChange = await UpdatePlayerRankingAsync(match.Player1UserName, 
                winnerUserName == match.Player1UserName, 
                isDraw, 
                match.Player1Score);

            int player2RankingChange = 0;
            if (!string.IsNullOrEmpty(match.Player2UserName))
            {
                player2RankingChange = await UpdatePlayerRankingAsync(match.Player2UserName,
                    winnerUserName == match.Player2UserName,
                    isDraw,
                    match.Player2Score);
            }

            // Clean up
            MatchAnswers.Remove(matchId);

            return new MatchResultDto
            {
                MatchId = matchId,
                WinnerUserName = winnerUserName,
                WinnerFullName = winnerUserName == match.Player1UserName 
                    ? match.Player1?.FullName 
                    : match.Player2?.FullName,
                IsDraw = isDraw,
                Player1Score = match.Player1Score,
                Player2Score = match.Player2Score,
                Player1UserName = match.Player1UserName,
                Player2UserName = match.Player2UserName,
                Player1FullName = match.Player1?.FullName ?? match.Player1UserName,
                Player2FullName = match.Player2?.FullName ?? match.Player2UserName,
                Player1RankingChange = player1RankingChange,
                Player2RankingChange = player2RankingChange
            };
        }

        private async Task<int> UpdatePlayerRankingAsync(string userName, bool isWinner, bool isDraw, int matchScore)
        {
            var ranking = await GetOrCreateDuelRankingAsync(userName);
            
            ranking.TotalMatches++;
            int scoreChange;

            if (isDraw)
            {
                ranking.Draws++;
                scoreChange = 15;
            }
            else if (isWinner)
            {
                ranking.Wins++;
                scoreChange = 30;
            }
            else
            {
                ranking.Losses++;
                scoreChange = 5;
            }

            ranking.TotalScore += scoreChange;
            await _context.SaveChangesAsync();

            return scoreChange;
        }

        public async Task HandlePlayerDisconnectAsync(string userName)
        {
            // Find active matches for this user
            var activeMatches = await _context.DuelMatches
                .Where(m => (m.Player1UserName == userName || m.Player2UserName == userName)
                         && m.Status == MatchStatus.InProgress)
                .ToListAsync();

            foreach (var match in activeMatches)
            {
                if (match.Player1UserName == userName)
                    match.Player1DisconnectedAt = DateTime.Now;
                else
                    match.Player2DisconnectedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ReconnectResultDto> HandlePlayerReconnectAsync(int matchId, string userName)
        {
            var match = await GetMatchByIdAsync(matchId);
            if (match == null)
                return new ReconnectResultDto { Success = false, Message = "Không tìm thấy trận đấu." };

            // Check if player is part of this match
            bool isPlayer1 = match.Player1UserName == userName;
            bool isPlayer2 = match.Player2UserName == userName;
            if (!isPlayer1 && !isPlayer2)
                return new ReconnectResultDto { Success = false, Message = "Bạn không phải người chơi trong trận này." };

            // Check reconnect timeout (30 seconds)
            var disconnectedAt = isPlayer1 ? match.Player1DisconnectedAt : match.Player2DisconnectedAt;
            if (disconnectedAt.HasValue && (DateTime.Now - disconnectedAt.Value).TotalSeconds > 30)
            {
                // Too late - match should be forfeit
                return new ReconnectResultDto { Success = false, Message = "Đã quá thời gian kết nối lại (30 giây)." };
            }

            // Clear disconnect time
            if (isPlayer1)
                match.Player1DisconnectedAt = null;
            else
                match.Player2DisconnectedAt = null;

            await _context.SaveChangesAsync();

            var currentQuestion = await GetCurrentQuestionAsync(matchId);
            var questions = match.QuestionSet?.Questions?.ToList();

            return new ReconnectResultDto
            {
                Success = true,
                MatchState = new MatchStateDto
                {
                    MatchId = matchId,
                    MatchCode = match.MatchCode,
                    Player1UserName = match.Player1UserName,
                    Player2UserName = match.Player2UserName,
                    Player1Score = match.Player1Score,
                    Player2Score = match.Player2Score,
                    CurrentQuestionIndex = match.CurrentQuestionIndex,
                    TotalQuestions = questions?.Count ?? 0,
                    Status = match.Status.ToString(),
                    CurrentQuestion = currentQuestion
                }
            };
        }

        public async Task HandlePlayerLeaveAsync(int matchId, string userName)
        {
            var match = await _context.DuelMatches.FindAsync(matchId);
            if (match == null) return;

            if (match.Status == MatchStatus.Waiting)
            {
                // If waiting, cancel the match
                if (match.Player1UserName == userName)
                {
                    match.Status = MatchStatus.Cancelled;
                }
                else if (match.Player2UserName == userName)
                {
                    match.Player2UserName = null;
                }
            }
            else if (match.Status == MatchStatus.InProgress)
            {
                // Player forfeits - other player wins
                match.Status = MatchStatus.Completed;
                match.EndedAt = DateTime.Now;
                match.WinnerUserName = match.Player1UserName == userName 
                    ? match.Player2UserName 
                    : match.Player1UserName;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<DuelMatch>> GetUserMatchHistoryAsync(string userName, int take = 10)
        {
            return await _context.DuelMatches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.QuestionSet)
                .Where(m => (m.Player1UserName == userName || m.Player2UserName == userName)
                         && m.Status == MatchStatus.Completed)
                .OrderByDescending(m => m.EndedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<DuelRanking>> GetDuelRankingAsync(int take = 50)
        {
            return await _context.DuelRankings
                .Include(r => r.User)
                .OrderByDescending(r => r.TotalScore)
                .ThenByDescending(r => r.Wins)
                .Take(take)
                .ToListAsync();
        }

        public async Task<DuelRanking> GetOrCreateDuelRankingAsync(string userName)
        {
            var ranking = await _context.DuelRankings.FindAsync(userName);
            if (ranking == null)
            {
                ranking = new DuelRanking { UserName = userName };
                _context.DuelRankings.Add(ranking);
                await _context.SaveChangesAsync();
            }
            return ranking;
        }

        public async Task<int> GetRandomQuestionSetIdAsync()
        {
            var questionSets = await _context.QuestionSets
                .Where(qs => qs.Questions.Count > 0)
                .Select(qs => qs.QSetId)
                .ToListAsync();

            if (questionSets.Count == 0)
                throw new InvalidOperationException("Không có bộ câu hỏi nào.");

            return questionSets[_random.Next(questionSets.Count)];
        }

        private static string GenerateMatchCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Avoid confusing characters
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public async Task<List<QuestionSetOptionDto>> GetAllQuestionSetsForSelectAsync()
        {
            return await _context.QuestionSets
                .Include(qs => qs.Category)
                .Include(qs => qs.Level)
                .Include(qs => qs.Questions)
                .Where(qs => qs.Questions.Count > 0)
                .Select(qs => new QuestionSetOptionDto
                {
                    QSetId = qs.QSetId,
                    QSetName = qs.QSetName,
                    CategoryName = qs.Category.CategoryName,
                    LevelName = qs.Level.LevelName,
                    QuestionCount = qs.Questions.Count
                })
                .ToListAsync();
        }
    }
}
