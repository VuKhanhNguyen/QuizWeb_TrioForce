using Microsoft.EntityFrameworkCore;
using quizweb.Data;
using quizweb.DTOs;
using quizweb.Models;
using quizweb.Repositories.Interfaces;

namespace quizweb.Repositories.Implementations
{
    public class QuestionSetRepository : IQuestionSetRepository
    {
        private readonly AppDbContext _context;

        public QuestionSetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddQuestionSetAsync(QuestionSet questionSet)
        {
            await _context.QuestionSets.AddAsync(questionSet);
            //await _context.SaveChangesAsync(); disable because using UoW
        }

        public void DeleteQuestionSetAsync(QuestionSet questionSet)
        {
                _context.QuestionSets.Remove(questionSet);
                //await _context.SaveChangesAsync(); disable because using UoW
        }

        public async Task<List<QuestionSet>> GetAllCreatedQuestionSetsByUsernameAsync(string username)
        {
            return await _context.QuestionSets.AsNoTracking().Where(qs => qs.AuthorName == username).ToListAsync();
        }

        public async Task<QuestionSet?> GetQuestionSetByIdAsync(int id)
        {
            return await _context.QuestionSets
                .AsNoTracking()
                .Include(qs => qs.Questions)
                    .ThenInclude(q => q.Answers)
                .Where(qs => qs.QSetId == id)
                .FirstOrDefaultAsync();
        }

        public async Task<QuestionSet?> GetQuestionSetRandomByNewGuidAsync()
        {
            return await _context.QuestionSets
                .AsNoTracking()
                .Include(qs => qs.Category)
                .Include(qs => qs.Level)
                .Include(qs => qs.Questions)
                    .ThenInclude(q => q.Answers)
                .OrderBy(q => Guid.NewGuid()).FirstOrDefaultAsync();
        }

        public async Task<QuestionSet?> GetQuestionSetRandomByIdCateAndIdLevel(int idCate, int idLevel)
        {
            var count = await _context.QuestionSets.CountAsync(qs => qs.CategoryId == idCate && qs.LevelId == idLevel);
            if (count == 0)
            {
                return null;
            }

            var rand = new Random();
            var randIndex = rand.Next(count);

            return await _context.QuestionSets
                .AsNoTracking()
                .Where(qs => qs.CategoryId == idCate && qs.LevelId == idLevel)
                .Skip(randIndex)
                .FirstOrDefaultAsync();
        }

        public void UpdateQuestionSetAsync(QuestionSet questionSet)
        {
            _context.QuestionSets.Update(questionSet);
            //await _context.SaveChangesAsync(); disable because using UoW
        }

        public async Task<List<CorrectAnswerDTO>>GetCorrectAnswerSetByIdAsync(int id)
        {
            return await _context.Questions
                .AsNoTracking()
                .Where(q => q.QSetId == id)
                .Select(q => new CorrectAnswerDTO
                {
                    QuestionId = q.QuestionId,
                    CorrectAnswerIds = q.Answers
                        .Where(a => a.IsCorrect)
                        .Select(a => a.AnswerId)
                        .ToHashSet()
                }).ToListAsync();
        }
    }
}
