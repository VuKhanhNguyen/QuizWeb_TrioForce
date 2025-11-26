using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;
using QuizWeb_TrioForce.Services.Interfaces;

namespace QuizWeb_TrioForce.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<QuestionSet>> GetCreatedQuestionSetsAsync(string username)
        {
            return await _userRepository.GetCreatedQuestionSetsAsync(username);
        }

        public async Task<List<MarkedQuestion>> GetMarkedQuestionsAsync(string username)
        {
            return await _userRepository.GetMarkedQuestionAsync(username);
        }

        public async Task<List<ProgressQuestionSet>> GetProgressQuestionSetsAsync(string username)
        {
            return await _userRepository.GetProgressQuestionSetsAsync(username);
        }

        public async Task<ApplicationUser> GetProfileAsync(string username)
        {
            var profile = await _userRepository.GetProfileAsync(username);
            
            return profile ?? throw new Exception("Profile is not found");
        }

        public async Task UpdateProfileAsync(ApplicationUser user)
        {
            await _userRepository.UpdateProfileAsync(user);
        }

        public async Task<int> GetTotalGamesPlayedAsync(string username)
        {
            var user = await _userRepository.GetProfileAsync(username);
            if (user == null) return 0;
            
            // Đếm số bộ câu hỏi unique mà user đã trả lời ít nhất 1 câu
            var uniqueQuestionSets = user.AnsweredQuestions
                .Select(aq => aq.QSetId)
                .Distinct()
                .Count();
            
            return uniqueQuestionSets;
        }

        public async Task<(int total, int correct)> GetAnswerStatsAsync(string username)
        {
            var user = await _userRepository.GetProfileAsync(username);
            if (user == null) return (0, 0);
            
            var totalAnswered = user.AnsweredQuestions.Count;
            var correctAnswers = user.AnsweredQuestions.Count(aq => aq.Answer.IsCorrect);
            
            return (totalAnswered, correctAnswers);
        }
    }

}
