using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.ViewModels.BookMark;

namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IQuestionSetService
    {
        public Task<QuestionSet> GetQuestionSetRandomByIdCateAndIdLevel(int idCate, int idLevel);
        public Task<List<CreatedQuestionSetListViewModel>> GetAllCreatedQuestionSetsAsync(string username);
        public Task<QuestionSet> GetQuestionSetByIdAsync(int id);

    }
}
