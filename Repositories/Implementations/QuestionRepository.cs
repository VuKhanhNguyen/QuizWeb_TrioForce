using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using quizweb.Data;
using quizweb.Models;
using quizweb.Repositories.Interfaces;

namespace quizweb.Repositories.Implementations
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddQuestionsAsync(List<Question> questions)
        {
            await _context.Questions.AddRangeAsync(questions);
        }

        public async Task AddQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
        }

        public void DeleteQuestionAsync(Question question)
        {
            _context.Questions.Remove(question);
        }

        public void DeleteQuestionsAsync(List<Question> questions)
        {
            _context.Questions.RemoveRange(questions);
        }

        public async Task<List<Question>> GetAllQuestionsByIdQSetAsync(int idQSet)
        {
            return await _context.Questions.AsNoTracking().Where(q => q.QSetId == idQSet).ToListAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await _context.Questions.FindAsync(id);
        }

        public void UpdateQuestionAsync(Question question)
        {
            _context.Questions.Update(question);
        }
    }
}
