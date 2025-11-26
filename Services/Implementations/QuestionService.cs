using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;
using QuizWeb_TrioForce.Services.Interfaces;

namespace QuizWeb_TrioForce.Services.Implementations
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionService(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<Question> GetQuestionByIdAsync(int id)
        {
             
            var q = await _questionRepository.GetQuestionByIdAsync(id);
            return q ?? throw new Exception("Question not found");
        }
    }
}
