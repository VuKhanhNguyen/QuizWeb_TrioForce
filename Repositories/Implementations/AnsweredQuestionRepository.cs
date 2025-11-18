using Microsoft.EntityFrameworkCore;
using quizweb.Data;
using quizweb.Models;
using quizweb.Repositories.Interfaces;

namespace quizweb.Repositories.Implementations
{
    public class AnsweredQuestionRepository : IAnsweredQuestionRepository
    {
        private readonly AppDbContext _context;

        public AnsweredQuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAnsweredQuestionAsync(AnsweredQuestion answeredQuestion)
        {
            await _context.AnsweredQuestions.AddAsync(answeredQuestion);
            await _context.SaveChangesAsync();
        }

        public async Task AddAnsweredQuestionsAsync(List<AnsweredQuestion> answeredQuestions)
        {
            await _context.AnsweredQuestions.AddRangeAsync(answeredQuestions);
        }

        public async Task<List<AnsweredQuestion>> GetAllAnsweredQuestions(string username, int QSetId)
        {
            return await _context.AnsweredQuestions.Where(aq => aq.UserName == username && aq.QSetId == QSetId).ToListAsync();
        }

        public async Task<AnsweredQuestion?> GetAnsweredQuestionById(int qId)
        {
            return await _context.AnsweredQuestions.Where(aq => aq.QuestionId == qId).FirstOrDefaultAsync();
        }

        public async Task RemoveAnsweredQuestionAsync(AnsweredQuestion answeredQuestion)
        {
            _context.AnsweredQuestions.Remove(answeredQuestion);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAnsweredQuestionAsync(AnsweredQuestion answeredQuestion)
        {
            _context.AnsweredQuestions.Update(answeredQuestion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAnsweredQuestionsAsync(List<AnsweredQuestion> answeredQuestions)
        {
            _context.AnsweredQuestions.UpdateRange(answeredQuestions);
            await _context.SaveChangesAsync();
        }
    }
}
