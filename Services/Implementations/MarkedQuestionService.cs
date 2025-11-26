using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.BookMark;

namespace QuizWeb_TrioForce.Services.Implementations
{
    public class MarkedQuestionService : IMarkedQuestionService
    {
        private readonly IMarkedQuestionRepository _markedQuestionRepository;

        public MarkedQuestionService(IMarkedQuestionRepository markedQuestionRepository)
        {
            _markedQuestionRepository = markedQuestionRepository;
        }

        public async Task<List<MarkedQuestionListViewModel>> GetAllMarkedQuestionsAsync(string username)
        {
            var allMarkedQuestions = await _markedQuestionRepository.GetAllMarkedQuestionsAsync(username);
            return allMarkedQuestions.Select(mq => new MarkedQuestionListViewModel()
            {
                QuestionId = mq.QuestionId,
                MarkedTime = mq.MarkedTime
            }).ToList();

        }

        public async Task AddMarkedQuestion(string username, int questionId)
        {
            var mq = new MarkedQuestion()
            {
                QuestionId = questionId,
                UserName = username,
                MarkedTime = DateTime.UtcNow
            };
            await _markedQuestionRepository.AddMarkedQuestion(mq);
        }

        public async Task RemoveMarkedQuestion(string username, int questionId)
        {
            var mq = await _markedQuestionRepository.GetMarkedQuestionByIdAsync(username, questionId);
            if (mq != null)
            {
                await _markedQuestionRepository.RemoveMarkedQuestion(mq);
            }
        }
    }
}
