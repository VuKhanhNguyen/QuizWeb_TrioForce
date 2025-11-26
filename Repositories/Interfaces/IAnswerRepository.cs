using QuizWeb_TrioForce.Models;

namespace QuizWeb_TrioForce.Repositories.Interfaces
{
    public interface IAnswerRepository
    {
        Task<Answer?> GetAnswerByIdAsync(int id);
        Task<List<Answer>> GetAllAnswersByIdQuestionAsync(int idQuestion);
        Task<List<Answer>> GetAllAnswersByQSetIdAsync(int idQSet);
        Task AddAnswersAsync(List<Answer> answers);
        Task AddAnswerAsync(Answer answer);
        void UpdateAnswerAsync(Answer answer);
        void DeleteAnswerAsync(Answer answer);
        void DeleteAnswersAsync(List<Answer> answers);
    }
}
