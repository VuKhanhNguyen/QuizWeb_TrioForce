using QuizWeb_TrioForce.Models;

namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IQuestionService
    {
        public Task<Question> GetQuestionByIdAsync(int id);
    }
}
