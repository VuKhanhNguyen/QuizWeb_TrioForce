using quizweb.DTOs;
using quizweb.Models;
using quizweb.ViewModels.User;

namespace quizweb.Services.Interfaces
{
    public interface IAnsweredQuestionService
    {
        Task<List<AnsweredQuestionDTO>> GetAllAnsweredQuestions(string username, int Qsetid);
        Task<AnsweredQuestion?> GetAnsweredQuestionById(int qId);

        Task SaveAnsweredQuestions(string username, int QsetId, List<UserAnswerViewModel> answeredQuestionsVm);
        Task AddAnsweredQuestions(List<AnsweredQuestion> answeredQuestions);
        Task UpdateAnsweredQuestions(List<AnsweredQuestion> answeredQuestions);
        

        Task AddAnsweredQuestion(AnsweredQuestion answeredQuestion);
        Task UpdateAnsweredQuestion(AnsweredQuestion answeredQuestion);
        Task RemoveAnsweredQuestion(int aqId);
    }
}
