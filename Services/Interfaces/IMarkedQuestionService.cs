using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.ViewModels.BookMark;
namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IMarkedQuestionService
    {
        public Task<List<MarkedQuestionListViewModel>> GetAllMarkedQuestionsAsync(string username);
        public Task AddMarkedQuestion(string username, int questionId);
        public Task RemoveMarkedQuestion(string username, int questionId);

    }
}
