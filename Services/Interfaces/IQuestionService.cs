using quizweb.Models;

namespace quizweb.Services.Interfaces
{
    public interface IQuestionService
    {
        public Task<Question> GetQuestionByIdAsync(int id);
    }
}
