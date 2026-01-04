using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.Duel;
using System.Security.Cryptography;
using System.Collections.Concurrent;

namespace QuizWeb_TrioForce.Services.Implementations
{
    public class DuelService : IDuelService
    {
        private readonly IDuelRepository _repo;
        private readonly ILogger<DuelService> _logger;

        // Track which players have answered current question (in production, use distributed cache)
        //private static readonly Dictionary<int, HashSet<string>> MatchAnswers = new();
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, byte>> MatchAnswers = new();

        public DuelService(IDuelRepository repo, ILogger<DuelService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<DuelMatch> CreateMatchAsync(string userName, int qSetId)
        {
            if (qSetId <= 0)
                qSetId = await _repo.GetRandomQuestionSetIdAsync();

            var match = new DuelMatch
            {
                MatchCode = GenerateMatchCode(),
                QSetId = qSetId,
                Player1UserName = userName,
                Status = MatchStatus.Waiting,
                CreatedAt = DateTime.Now
            };

            await _repo.AddMatchAsync(match);
            await _repo.SaveChangesAsync();

            return await _repo.GetMatchWithQuestionsAsync(match.MatchId) ?? match;
        }

        public async Task<DuelMatch?> JoinMatchAsync(string matchCode, string userName)
        {
            var match = await _repo.GetWaitingMatchForJoinAsync(matchCode);
            if (match == null) return null;
            if (match.Player1UserName == userName) return null;

            match.Player2UserName = userName;

            await _repo.SaveChangesAsync();
            return await _repo.GetMatchForLobbyByIdAsync(match.MatchId);
        }

        public Task<DuelMatch?> GetMatchByIdAsync(int matchId)
            => _repo.GetMatchWithQuestionsAsync(matchId);

        public Task<DuelMatch?> GetMatchByCodeAsync(string matchCode)
            => _repo.GetMatchForLobbyByCodeAsync(matchCode);


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
            MatchAnswers[matchId] = new ConcurrentDictionary<string, byte>();

            await _repo.SaveChangesAsync();
            return match;
        }

        public async Task<QuestionDataDto?> GetCurrentQuestionAsync(int matchId)
        {
            var match = await _repo.GetMatchWithQuestionsAsync(matchId);
            if (match == null || match.QuestionSet?.Questions == null) return null;

            var questions = match.QuestionSet.Questions.OrderBy(q => q.QuestionId).ToList();
            if (match.CurrentQuestionIndex >= questions.Count) return null;

            var question = questions[match.CurrentQuestionIndex];

            MatchAnswers.GetOrAdd(matchId, _ => new ConcurrentDictionary<string, byte>());

            return new QuestionDataDto
            {
                QuestionIndex = match.CurrentQuestionIndex + 1,
                TotalQuestions = questions.Count,
                QuestionId = question.QuestionId,
                QuestionText = question.QuestionText,
                TimeLimit = 10,
                Answers = question.Answers
                    .Select(a => new AnswerOptionDto
                    {
                        AnswerId = a.AnswerId,
                        AnswerText = a.AnswerText
                    })
                    .OrderBy(_ => Random.Shared.Next())
                    .ToList()
            };
        }

        public async Task<AnswerResultDto> SubmitAnswerAsync(int matchId, string userName, int questionId, int answerId, double responseTime)
        {

            var match = await _repo.GetMatchWithQuestionsAsync(matchId)
                ?? throw new InvalidOperationException("Không tìm thấy trận đấu.");

            var question = match.QuestionSet?.Questions?.FirstOrDefault(q => q.QuestionId == questionId);
            var correctAnswer = question?.Answers?.FirstOrDefault(a => a.IsCorrect);
            bool isCorrect = answerId == correctAnswer?.AnswerId;

            int scoreEarned = 0;
            if (isCorrect && answerId > 0)
                scoreEarned = 15 + Math.Max(0, (int)(5 - responseTime * 0.5));

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

            await _repo.AddAnswerAsync(duelAnswer);

            if (userName == match.Player1UserName) match.Player1Score += scoreEarned;
            else match.Player2Score += scoreEarned;

            await _repo.SaveChangesAsync();

            var answeredUsers = MatchAnswers.GetOrAdd(matchId, _ => new ConcurrentDictionary<string, byte>());

            answeredUsers.TryAdd(userName, 0);

            bool bothAnswered = false;
            bool isLastQuestion = false;

            // CHỈ LOCK ĐOẠN LOGIC CHUYỂN CÂU HỎI (Rất nhanh, không ảnh hưởng hiệu năng)
            lock (answeredUsers)
            {
                // Kiểm tra lại count trong lock để đảm bảo chính xác tuyệt đối
                bothAnswered = answeredUsers.Count >= 2;

                var questions = match.QuestionSet?.Questions?.OrderBy(q => q.QuestionId).ToList();
                isLastQuestion = match.CurrentQuestionIndex >= (questions?.Count ?? 0) - 1;

                if (bothAnswered && !isLastQuestion)
                {
                    // Reset ngay trong lock để các thread khác đến sau thấy Count = 0
                    answeredUsers.Clear();

                    // Đánh dấu để cập nhật DB bên dưới
                    match.CurrentQuestionIndex++;
                }
            }

            if (bothAnswered && !isLastQuestion)
            {
                await _repo.SaveChangesAsync();
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

            await _repo.SaveChangesAsync();

            // Update rankings
            var player1RankingChange = await UpdatePlayerRankingAsync(
                match.Player1UserName,
                winnerUserName == match.Player1UserName,
                isDraw);

            int player2RankingChange = 0;
            if (!string.IsNullOrEmpty(match.Player2UserName))
            {
                player2RankingChange = await UpdatePlayerRankingAsync(
                    match.Player2UserName,
                    winnerUserName == match.Player2UserName,
                    isDraw);
            }


            // Clean up
            MatchAnswers.TryRemove(matchId, out _);

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

        private async Task<int> UpdatePlayerRankingAsync(string userName, bool isWinner, bool isDraw)
        {
            var ranking = await _repo.GetOrCreateRankingAsync(userName);

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
            await _repo.SaveChangesAsync();

            return scoreChange;
        }



        public async Task HandlePlayerDisconnectAsync(string userName)
        {
            var activeMatches = await _repo.GetInProgressMatchesForUserAsync(userName);

            foreach (var match in activeMatches)
            {
                if (match.Player1UserName == userName) match.Player1DisconnectedAt = DateTime.Now;
                else match.Player2DisconnectedAt = DateTime.Now;
            }

            await _repo.SaveChangesAsync();
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

            await _repo.SaveChangesAsync();

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
            var match = await _repo.FindMatchAsync(matchId);
            if (match == null) return;

            if (match.Status == MatchStatus.Waiting)
            {
                if (match.Player1UserName == userName) match.Status = MatchStatus.Cancelled;
                else if (match.Player2UserName == userName) match.Player2UserName = null;
            }
            else if (match.Status == MatchStatus.InProgress)
            {
                match.Status = MatchStatus.Completed;
                match.EndedAt = DateTime.Now;
                match.WinnerUserName = match.Player1UserName == userName ? match.Player2UserName : match.Player1UserName;
            }

            await _repo.SaveChangesAsync();
        }


        public Task<List<DuelMatch>> GetUserMatchHistoryAsync(string userName, int take = 10)
            => _repo.GetUserMatchHistoryAsync(userName, take);


        public Task<List<DuelRanking>> GetDuelRankingAsync(int take = 50)
            => _repo.GetTopRankingsAsync(take);


        private static string GenerateMatchCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Span<char> code = stackalloc char[6];

            for (int i = 0; i < code.Length; i++)
            {
                code[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }

            return new string(code);
        }


        public Task<List<QuestionSetOptionDto>> GetAllQuestionSetsForSelectAsync()
            => _repo.GetAllQuestionSetsForSelectAsync();


        public Task<List<DuelMatch>> GetActiveMatchesForUserAsync(string userName)
            => _repo.GetActiveMatchesForUserAsync(userName);

        public async Task EndMatchDueToDisconnectAsync(int matchId, string disconnectedPlayerUserName)
        {
            var match = await GetMatchByIdAsync(matchId);
            if (match == null || match.Status != MatchStatus.InProgress)
                return;

            match.Status = MatchStatus.Completed;
            match.EndedAt = DateTime.Now;

            // The opponent wins
            match.WinnerUserName = match.Player1UserName == disconnectedPlayerUserName
                ? match.Player2UserName
                : match.Player1UserName;

            await _repo.SaveChangesAsync();

            // Update rankings
            var winnerUserName = match.WinnerUserName;
            var loserUserName = disconnectedPlayerUserName;

            if (!string.IsNullOrEmpty(winnerUserName))
            {
                await UpdatePlayerRankingAsync(winnerUserName, true, false);
            }

            await UpdatePlayerRankingAsync(loserUserName, false, false);

            // Clean up
            MatchAnswers.TryRemove(matchId, out _);

            _logger.LogInformation("Match {MatchId} ended due to player {PlayerUserName} disconnect timeout. Winner: {WinnerUserName}", 
                matchId, disconnectedPlayerUserName, winnerUserName);
        }

        public Task<DuelRanking> GetOrCreateDuelRankingAsync(string userName) => _repo.GetOrCreateRankingAsync(userName);
    }
}
