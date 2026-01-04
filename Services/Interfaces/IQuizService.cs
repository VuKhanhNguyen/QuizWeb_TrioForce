using QuizWeb_TrioForce.ViewModels.ProgressQuestionSet;
using QuizWeb_TrioForce.ViewModels.QuestionSet;

namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IQuizService
    {
        Task CreateQuizAsync(CreateQuestionSetViewModel viewModel, string authorName);
        Task UpdateQuizAsync(UpdateQuestionSetViewModel viewModel, string authorName);
        Task<UpdateQuestionSetViewModel> GetQuizForEditAsync(int qSetId, string username);
        Task DeleteQuizAsync(int id, string username);

        Task<PlayQuestionSetViewModel> GetQuizAsync(int id);
        Task<QuizResultViewModel> SubmitQuizAsync(SubmitQuizViewModel submitModel, string username);
        Task SaveProgressAsync(SaveProgressViewModel saveModel, string username);
        Task<PlayQuestionSetViewModel> GetRandomQuizAsync();
        Task<PlayQuestionSetViewModel> GetQuizByCategoryAndLevelAsync(int categoryId, int levelId);
        Task <ResumeQuestionSetViewModel> LoadProgressAsync(string username, int qSetId);
    }
}
