namespace quizweb.Repositories.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IQuestionSetRepository QuestionSetRepository { get; }
        IQuestionRepository QuestionRepository { get; }
        IAnswerRepository AnswerRepository { get; }
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveChangesAsync();
    }
}
