using quizweb.DTOs;
using quizweb.Models;

namespace quizweb.Repositories.Interfaces
{
    public interface IQuestionSetRepository
    {
        Task<QuestionSet?> GetQuestionSetByIdAsync(int id);
        Task<QuestionSet?> GetQuestionSetRandomByNewGuidAsync();
        Task<QuestionSet?> GetQuestionSetRandomByIdCateAndIdLevel(int idCate, int idLevel);
        Task<List<CorrectAnswerDTO>> GetCorrectAnswerSetByIdAsync(int id);
        Task<List<QuestionSet>> GetAllCreatedQuestionSetsByUsernameAsync(string username);
        Task AddQuestionSetAsync(QuestionSet questionSet);
        void UpdateQuestionSetAsync(QuestionSet questionSet);
        void DeleteQuestionSetAsync(QuestionSet questionSet);
    }
}
