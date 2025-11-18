using QuizWeb_TrioForce.Models;

namespace QuizWeb_TrioForce.Repositories.Interfaces
{
    public interface IMarkedQuestionRepository
    {
        Task<List<MarkedQuestion>> GetAllMarkedQuestionsAsync(string username);
        Task<MarkedQuestion?> GetMarkedQuestionByIdAsync(string username, int questionId);
        Task AddMarkedQuestion(MarkedQuestion markedQuestion);
        Task RemoveMarkedQuestion(MarkedQuestion markedQuestion);
    }
}
