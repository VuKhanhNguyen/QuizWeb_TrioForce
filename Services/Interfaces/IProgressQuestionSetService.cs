using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.ViewModels.BookMark;
using QuizWeb_TrioForce.ViewModels.ProgressQuestionSet;

namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IProgressQuestionSetService
    {
        Task AddProgressQuestionSet(ProgressQuestionSetViewModel viewModel, string username);
        Task UpdateProgressQuestionSet(ProgressQuestionSetViewModel viewModel, string username);
        Task DeleteProgressQuestionSet(string username, int QSetId);
        Task<List<ProgressQuestionSetListViewModel>> GetAllProgressQuestionSets(string username);
        Task<ProgressQuestionSet?> GetProgressQuestionSetByUsernameAndQSetId(string username, int QSetId);


    }
}
