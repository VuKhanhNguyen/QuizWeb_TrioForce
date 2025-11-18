using quizweb.Models;

namespace quizweb.Repositories.Interfaces
{
    public interface IProgressQuestionSetRepository
    {
        Task<List<ProgressQuestionSet>> GetAllProgressQuestionSets(string username);
        Task<ProgressQuestionSet?> GetProgressQuestionSetByUsernameAndQSetId(string username, int id);
        Task AddProgressQuestionSet(ProgressQuestionSet progressQuestionSet);
        Task UpdateProgressQuestionSet(ProgressQuestionSet progressQuestionSet);
        Task DeleteProgressQuestionSet(ProgressQuestionSet progressQuestionSet);

    }
}
