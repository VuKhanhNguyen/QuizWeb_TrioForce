using QuizWeb_TrioForce.Models;

namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IUserService
    {
        public Task<List<QuestionSet>> GetCreatedQuestionSetsAsync(string username);
        public Task<List<MarkedQuestion>> GetMarkedQuestionsAsync(string username);
        public Task<List<ProgressQuestionSet>> GetProgressQuestionSetsAsync(string username);
        public Task<ApplicationUser> GetProfileAsync(string username);
        public Task UpdateProfileAsync(ApplicationUser user);

    }
}
