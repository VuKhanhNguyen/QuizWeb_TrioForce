using QuizWeb_TrioForce.DTOs;
using QuizWeb_TrioForce.Models;

namespace QuizWeb_TrioForce.Repositories.Interfaces
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
