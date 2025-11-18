using Microsoft.EntityFrameworkCore;
using QuizWeb_TrioForce.Data;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;
using SQLitePCL;

namespace QuizWeb_TrioForce.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {

        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuestionSet>> GetCreatedQuestionSetsAsync(string username)

        {
            return await _context.QuestionSets.AsNoTracking().Where(qs => qs.AuthorName == username).ToListAsync();
        }

        public async Task<List<MarkedQuestion>> GetMarkedQuestionAsync(string username)

        {
            return await _context.MarkedQuestions.AsNoTracking().Where(mq => mq.UserName == username).ToListAsync();
        }

        public async Task<List<ProgressQuestionSet>> GetProgressQuestionSetsAsync(string username)

        {
            return await _context.ProgressQuestionSets.AsNoTracking().Where(pqs => pqs.UserName == username).ToListAsync();
        }

        public async Task<ApplicationUser?> GetProfileAsync(string username)

        {
            return await _context.Users.AsNoTracking().Where(u => u.UserName == username).FirstOrDefaultAsync();
        }

        public async Task UpdateProfileAsync(ApplicationUser user)

        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
