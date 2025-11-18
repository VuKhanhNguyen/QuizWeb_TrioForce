using quizweb.Models;
using quizweb.ViewModels.BookMark;
namespace quizweb.Services.Interfaces
{
    public interface IMarkedQuestionService
    {
        public Task<List<MarkedQuestionListViewModel>> GetAllMarkedQuestionsAsync(string username);
        public Task AddMarkedQuestion(string username, int questionId);
        public Task RemoveMarkedQuestion(string username, int questionId);

    }
}
