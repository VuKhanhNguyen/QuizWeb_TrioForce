using quizweb.Models;
using quizweb.ViewModels.BookMark;

namespace quizweb.Services.Interfaces
{
    public interface IQuestionSetService
    {
        public Task<QuestionSet> GetQuestionSetRandomByIdCateAndIdLevel(int idCate, int idLevel);
        public Task<List<CreatedQuestionSetListViewModel>> GetAllCreatedQuestionSetsAsync(string username);
        public Task<QuestionSet> GetQuestionSetByIdAsync(int id);

    }
}
