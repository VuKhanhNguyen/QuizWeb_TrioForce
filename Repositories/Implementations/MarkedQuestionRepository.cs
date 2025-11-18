using Microsoft.EntityFrameworkCore;
using QuizWeb_TrioForce.Data;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;

namespace QuizWeb_TrioForce.Repositories.Implementations
{
    public class MarkedQuestionRepository : IMarkedQuestionRepository
    {
        private readonly AppDbContext _context;

        public MarkedQuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddMarkedQuestion(MarkedQuestion markedQuestion)
        {
            await _context.MarkedQuestions.AddAsync(markedQuestion);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MarkedQuestion>> GetAllMarkedQuestionsAsync(string username)
        {
            return await _context.MarkedQuestions.AsNoTracking().Where(mq => mq.UserName == username).ToListAsync();
        }

        public async Task<MarkedQuestion?> GetMarkedQuestionByIdAsync(string username, int questionId)
        {
            return await _context.MarkedQuestions.AsNoTracking().Where(mq => (mq.QuestionId == questionId && mq.UserName == username)).FirstOrDefaultAsync();
        }

        public async Task RemoveMarkedQuestion(MarkedQuestion markedQuestion)
        {
                _context.MarkedQuestions.Remove(markedQuestion);
                await _context.SaveChangesAsync();
        }
    }
}
