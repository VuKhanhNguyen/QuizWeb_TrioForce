using QuizWeb_TrioForce.Models;

namespace QuizWeb_TrioForce.Repositories.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Question?> GetQuestionByIdAsync(int id);
        Task<List<Question>> GetAllQuestionsByIdQSetAsync(int idQSet);
        Task AddQuestionsAsync(List<Question> questions);
        Task AddQuestionAsync(Question question);
        void UpdateQuestionAsync(Question question);
        void DeleteQuestionAsync(Question question);
        void DeleteQuestionsAsync(List<Question> questions);
    }
}
