using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.Data;

namespace QuizWeb_TrioForce.Hubs
{
    [Authorize]
    public class DuelHub : Hub
    {
        private readonly IDuelService _duelService;
        private readonly ILogger<DuelHub> _logger;
        private readonly AppDbContext _context;

        // Store connection mappings (in production, use Redis or a distributed cache)
        private static readonly Dictionary<string, string> UserConnections = new();
        private static readonly Dictionary<int, HashSet<string>> MatchConnections = new();

        public DuelHub(IDuelService duelService, ILogger<DuelHub> logger, AppDbContext context)
        {
            _duelService = duelService;
            _logger = logger;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                UserConnections[userName] = Context.ConnectionId;
                _logger.LogInformation("User {UserName} connected with ConnectionId {ConnectionId}", userName, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                UserConnections.Remove(userName);
                _logger.LogInformation("User {UserName} disconnected", userName);

                // Notify matches about disconnection (mark disconnect time in DB)
                await _duelService.HandlePlayerDisconnectAsync(userName);

                // Find active matches and notify opponent
                var activeMatches = await _duelService.GetActiveMatchesForUserAsync(userName);
                foreach (var match in activeMatches)
                {
                    // Notify opponent about disconnection - they will start their own 30s countdown
                    await Clients.Group($"match_{match.MatchId}")
                        .SendAsync("PlayerDisconnected", new { UserName = userName });
                    
                    _logger.LogInformation("Notified match {MatchId} about player {UserName} disconnect", match.MatchId, userName);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Tạo phòng đấu mới
        /// </summary>
        public async Task CreateRoom(int qSetId)
        {
            var userName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                await Clients.Caller.SendAsync("Error", "Bạn cần đăng nhập để tạo phòng.");
                return;
            }

            try
            {
                var match = await _duelService.CreateMatchAsync(userName, qSetId);
                
                // Add to match group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"match_{match.MatchId}");
                
                if (!MatchConnections.ContainsKey(match.MatchId))
                    MatchConnections[match.MatchId] = new HashSet<string>();
                MatchConnections[match.MatchId].Add(Context.ConnectionId);

                await Clients.Caller.SendAsync("RoomCreated", new
                {
                    MatchId = match.MatchId,
                    MatchCode = match.MatchCode,
                    QuestionSetName = match.QuestionSet?.QSetName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room for user {UserName}", userName);
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra khi tạo phòng.");
            }
        }

        /// <summary>
        /// Tham gia phòng đấu bằng mã
        /// </summary>
        public async Task JoinRoom(string matchCode)
        {
            var userName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                await Clients.Caller.SendAsync("Error", "Bạn cần đăng nhập để tham gia.");
                return;
            }

            try
            {
                var match = await _duelService.JoinMatchAsync(matchCode, userName);
                if (match == null)
                {
                    await Clients.Caller.SendAsync("Error", "Không tìm thấy phòng hoặc phòng đã đầy.");
                    return;
                }

                // Add to match group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"match_{match.MatchId}");
                
                if (!MatchConnections.ContainsKey(match.MatchId))
                    MatchConnections[match.MatchId] = new HashSet<string>();
                MatchConnections[match.MatchId].Add(Context.ConnectionId);

                // Notify the room owner that someone joined
                await Clients.Group($"match_{match.MatchId}").SendAsync("PlayerJoined", new
                {
                    MatchId = match.MatchId,
                    Player2UserName = userName,
                    Player2FullName = match.Player2?.FullName
                });

                // Notify the joiner about successful join
                await Clients.Caller.SendAsync("RoomJoined", new
                {
                    MatchId = match.MatchId,
                    MatchCode = match.MatchCode,
                    Player1UserName = match.Player1UserName,
                    Player1FullName = match.Player1?.FullName,
                    QuestionSetName = match.QuestionSet?.QSetName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room for user {UserName}", userName);
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra khi tham gia phòng.");
            }
        }

        /// <summary>
        /// Join vào SignalR group của match (gọi khi vào Lobby page)
        /// Không thay đổi database, chỉ join group để nhận events
        /// </summary>
        public async Task JoinMatchGroup(int matchId)
        {
            var userName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                await Clients.Caller.SendAsync("Error", "Bạn cần đăng nhập.");
                return;
            }

            try
            {
                var match = await _duelService.GetMatchByIdAsync(matchId);
                if (match == null)
                {
                    await Clients.Caller.SendAsync("Error", "Không tìm thấy phòng.");
                    return;
                }

                // Verify user is part of this match
                if (match.Player1UserName != userName && match.Player2UserName != userName)
                {
                    await Clients.Caller.SendAsync("Error", "Bạn không phải người chơi trong phòng này.");
                    return;
                }

                // Add to match group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"match_{matchId}");
                
                if (!MatchConnections.ContainsKey(matchId))
                    MatchConnections[matchId] = new HashSet<string>();
                MatchConnections[matchId].Add(Context.ConnectionId);

                _logger.LogInformation("User {UserName} joined match group {MatchId}", userName, matchId);

                // Check if this is a reconnect (player was disconnected)
                bool isPlayer1 = match.Player1UserName == userName;
                var wasDisconnected = isPlayer1 
                    ? match.Player1DisconnectedAt.HasValue 
                    : match.Player2DisconnectedAt.HasValue;

                // Clear disconnect time
                if (wasDisconnected)
                {
                    if (isPlayer1)
                        match.Player1DisconnectedAt = null;
                    else
                        match.Player2DisconnectedAt = null;

                    await _context.SaveChangesAsync();

                    // Notify opponent about reconnection
                    await Clients.GroupExcept($"match_{matchId}", Context.ConnectionId)
                        .SendAsync("PlayerReconnected", new { UserName = userName });
                    
                    _logger.LogInformation("User {UserName} reconnected to match {MatchId}", userName, matchId);
                }
                else
                {
                    // First time connection - notify others in the group that this player is connected
                    await Clients.GroupExcept($"match_{matchId}", Context.ConnectionId).SendAsync("PlayerConnected", new
                    {
                        UserName = userName,
                        IsPlayer1 = isPlayer1
                    });
                }

                // Send current match state back to the caller
                await Clients.Caller.SendAsync("JoinedMatchGroup", new
                {
                    MatchId = matchId,
                    MatchCode = match.MatchCode,
                    Player1UserName = match.Player1UserName,
                    Player1FullName = match.Player1?.FullName,
                    Player2UserName = match.Player2UserName,
                    Player2FullName = match.Player2?.FullName,
                    Status = match.Status.ToString(),
                    HasBothPlayers = !string.IsNullOrEmpty(match.Player2UserName)
                });

                // If match is already in progress, send current question to newly joined player
                if (match.Status == MatchStatus.InProgress)
                {
                    var currentQuestion = await _duelService.GetCurrentQuestionAsync(matchId);
                    if (currentQuestion != null)
                    {
                        await Clients.Caller.SendAsync("NewQuestion", currentQuestion);
                        _logger.LogInformation("Sent current question to rejoining player {UserName} in match {MatchId}", userName, matchId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining match group {MatchId} for user {UserName}", matchId, userName);
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra.");
            }
        }

        /// <summary>
        /// Bắt đầu trận đấu (chỉ Player1 có thể gọi)
        /// </summary>
        public async Task StartMatch(int matchId)
        {
            var userName = Context.User?.Identity?.Name;
            
            try
            {
                var match = await _duelService.StartMatchAsync(matchId, userName!);
                if (match == null)
                {
                    await Clients.Caller.SendAsync("Error", "Không thể bắt đầu trận đấu.");
                    return;
                }

                // Send first question to both players
                var questionData = await _duelService.GetCurrentQuestionAsync(matchId);
                
                await Clients.Group($"match_{matchId}").SendAsync("MatchStarted", new
                {
                    MatchId = matchId,
                    TotalQuestions = match.QuestionSet?.Questions?.Count ?? 0,
                    TimePerQuestion = 10 // seconds
                });

                await Clients.Group($"match_{matchId}").SendAsync("NewQuestion", questionData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting match {MatchId}", matchId);
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Player gửi câu trả lời
        /// </summary>
        public async Task SubmitAnswer(int matchId, int questionId, int answerId, double responseTime)
        {
            var userName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                await Clients.Caller.SendAsync("Error", "Bạn cần đăng nhập.");
                return;
            }

            try
            {
                var result = await _duelService.SubmitAnswerAsync(matchId, userName, questionId, answerId, responseTime);
                
                // Notify the player about their result
                await Clients.Caller.SendAsync("AnswerResult", new
                {
                    IsCorrect = result.IsCorrect,
                    ScoreEarned = result.ScoreEarned,
                    TotalScore = result.TotalScore
                });

                // Notify opponent that this player has answered (without revealing the answer)
                await Clients.GroupExcept($"match_{matchId}", Context.ConnectionId).SendAsync("OpponentAnswered", new
                {
                    UserName = userName
                });

                // Check if both players have answered
                if (result.BothPlayersAnswered)
                {
                    // Send question result to both players
                    await Clients.Group($"match_{matchId}").SendAsync("QuestionResult", new
                    {
                        CorrectAnswerId = result.CorrectAnswerId,
                        Player1Score = result.Player1TotalScore,
                        Player2Score = result.Player2TotalScore
                    });

                    // Check if match is over
                    if (result.IsMatchOver)
                    {
                        var matchResult = await _duelService.EndMatchAsync(matchId);
                        await Clients.Group($"match_{matchId}").SendAsync("MatchEnded", matchResult);
                    }
                    else
                    {
                        // Wait a bit then send next question
                        await Task.Delay(2000); // 2 second delay between questions
                        var nextQuestion = await _duelService.GetCurrentQuestionAsync(matchId);
                        await Clients.Group($"match_{matchId}").SendAsync("NewQuestion", nextQuestion);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answer for user {UserName} in match {MatchId}", userName, matchId);
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra khi gửi câu trả lời.");
            }
        }

        /// <summary>
        /// Xử lý timeout khi hết thời gian trả lời
        /// </summary>
        public async Task TimeOut(int matchId, int questionId)
        {
            var userName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return;

            // Submit with -1 to indicate timeout (no answer selected)
            await SubmitAnswer(matchId, questionId, -1, 10.0);
        }

        /// <summary>
        /// Player muốn rời phòng/hủy match
        /// </summary>
        public async Task LeaveMatch(int matchId)
        {
            var userName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return;

            try
            {
                await _duelService.HandlePlayerLeaveAsync(matchId, userName);
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match_{matchId}");
                
                if (MatchConnections.ContainsKey(matchId))
                    MatchConnections[matchId].Remove(Context.ConnectionId);

                // Notify the other player
                await Clients.Group($"match_{matchId}").SendAsync("PlayerLeft", new
                {
                    UserName = userName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving match {MatchId} for user {UserName}", matchId, userName);
            }
        }

        /// <summary>
        /// Reconnect sau khi bị disconnect
        /// </summary>
        public async Task Reconnect(int matchId)
        {
            var userName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return;

            try
            {
                var reconnectResult = await _duelService.HandlePlayerReconnectAsync(matchId, userName);
                
                if (reconnectResult.Success)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"match_{matchId}");
                    
                    if (!MatchConnections.ContainsKey(matchId))
                        MatchConnections[matchId] = new HashSet<string>();
                    MatchConnections[matchId].Add(Context.ConnectionId);

                    await Clients.Caller.SendAsync("Reconnected", reconnectResult.MatchState);
                    
                    await Clients.GroupExcept($"match_{matchId}", Context.ConnectionId)
                        .SendAsync("PlayerReconnected", new { UserName = userName });
                }
                else
                {
                    await Clients.Caller.SendAsync("ReconnectFailed", reconnectResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconnecting to match {MatchId} for user {UserName}", matchId, userName);
                await Clients.Caller.SendAsync("Error", "Có lỗi xảy ra khi kết nối lại.");
            }
        }

        /// <summary>
        /// Frontend gọi khi countdown disconnect timeout về 0
        /// </summary>
        public async Task NotifyDisconnectTimeout(int matchId)
        {
            var userName = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return;

            try
            {
                _logger.LogInformation("Disconnect timeout notification from {UserName} for match {MatchId}", userName, matchId);
                
                var match = await _duelService.GetMatchByIdAsync(matchId);
                if (match == null || match.Status != MatchStatus.InProgress)
                {
                    _logger.LogWarning("Match {MatchId} not found or not in progress", matchId);
                    return;
                }

                // Determine which player disconnected (the one who is NOT calling this method)
                string? disconnectedPlayer = null;
                if (match.Player1UserName == userName)
                {
                    // Player1 is calling, so Player2 is the disconnected one
                    if (match.Player2DisconnectedAt.HasValue)
                    {
                        disconnectedPlayer = match.Player2UserName;
                    }
                }
                else if (match.Player2UserName == userName)
                {
                    // Player2 is calling, so Player1 is the disconnected one
                    if (match.Player1DisconnectedAt.HasValue)
                    {
                        disconnectedPlayer = match.Player1UserName;
                    }
                }

                if (!string.IsNullOrEmpty(disconnectedPlayer))
                {
                    _logger.LogInformation("Ending match {MatchId} due to disconnect timeout. Disconnected player: {DisconnectedPlayer}", matchId, disconnectedPlayer);
                    
                    // End the match, awarding win to the player who is still connected
                    await _duelService.EndMatchDueToDisconnectAsync(matchId, disconnectedPlayer);
                }
                else
                {
                    _logger.LogInformation("No disconnected player found or player reconnected for match {MatchId}", matchId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling disconnect timeout notification for match {MatchId}", matchId);
            }
        }
    }
}
