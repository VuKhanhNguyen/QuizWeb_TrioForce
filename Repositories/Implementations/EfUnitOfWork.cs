using Microsoft.EntityFrameworkCore.Storage;
using quizweb.Data;
using quizweb.Models;
using quizweb.Repositories.Interfaces;

namespace quizweb.Repositories.Implementations
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        private bool _disposed;

        public EfUnitOfWork(AppDbContext context, IQuestionSetRepository qsRepo, IQuestionRepository qRepo, IAnswerRepository aRepo)
        {
            _context = context;
            QuestionSetRepository = qsRepo;
            QuestionRepository = qRepo;
            AnswerRepository = aRepo;
        }

        public IQuestionSetRepository QuestionSetRepository { get; }

        public IQuestionRepository QuestionRepository { get; }

        public IAnswerRepository AnswerRepository { get; }


        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                return;
            }
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("The transaction has not been initiated yet.");
            }

            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }

                await _context.DisposeAsync();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
