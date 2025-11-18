using Microsoft.AspNetCore.Mvc.RazorPages;
using quizweb.DTOs;
using quizweb.Models;
using quizweb.Repositories.Interfaces;
using quizweb.Services.Interfaces;
using quizweb.ViewModels.User;

namespace quizweb.Services.Implementations
{
    public class AnsweredQuestionService : IAnsweredQuestionService
    {
        private readonly IAnsweredQuestionRepository _answeredQuestion;

        public AnsweredQuestionService(IAnsweredQuestionRepository answeredQuestion)
        {
            _answeredQuestion = answeredQuestion;
        }

        public async Task AddAnsweredQuestion(AnsweredQuestion answeredQuestion)
        {
            await _answeredQuestion.AddAnsweredQuestionAsync(answeredQuestion);
        }

        public async Task AddAnsweredQuestions(List<AnsweredQuestion> answeredQuestions)
        {
            await _answeredQuestion.AddAnsweredQuestionsAsync(answeredQuestions);
        }

        public async Task<List<AnsweredQuestionDTO>> GetAllAnsweredQuestions(string username, int QSetId)
        {
            var answeredQuestions = await _answeredQuestion.GetAllAnsweredQuestions(username, QSetId);

            return answeredQuestions.Select(aQ => new AnsweredQuestionDTO()
            {
                QuestionId = aQ.QuestionId,
                SelectedAnswerId = aQ.SelectedAnswerId
            }).ToList();
        }

        public Task<AnsweredQuestion?> GetAnsweredQuestionById(int qId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveAnsweredQuestion(int AqId)
        {
            var answeredQuestion = await _answeredQuestion.GetAnsweredQuestionById(AqId);
            if (answeredQuestion != null)
            {
                await _answeredQuestion.RemoveAnsweredQuestionAsync(answeredQuestion);
            }
        }

        public async Task SaveAnsweredQuestions(string username, int QsetId, List<UserAnswerViewModel> answeredQuestionsVm)
        {
            var answeredQuestionList = await _answeredQuestion.GetAllAnsweredQuestions(username, QsetId);
            var answerQuestionDict = answeredQuestionList.ToDictionary(aq => aq.QuestionId);

            var questionToAdd = new List<AnsweredQuestion>();

            foreach (var ua in answeredQuestionsVm)
            {
                if (answerQuestionDict.TryGetValue(ua.QuestionId, out var existingAnswer))
                {
                    if (existingAnswer.SelectedAnswerId != ua.SelectedAnswerId)
                    {
                        existingAnswer.SelectedAnswerId = ua.SelectedAnswerId;
                    }
                }
                else
                {
                    questionToAdd.Add(new AnsweredQuestion()
                    {
                        UserName = username,
                        QSetId = QsetId,
                        QuestionId = ua.QuestionId,
                        SelectedAnswerId = ua.SelectedAnswerId
                    });
                }
            }

            if (questionToAdd.Any())
            {
                await _answeredQuestion.AddAnsweredQuestionsAsync(questionToAdd);
            }

            await _answeredQuestion.SaveChangeAsync();
        }

        public async Task UpdateAnsweredQuestion(AnsweredQuestion answeredQuestion)
        {
            await _answeredQuestion.UpdateAnsweredQuestionAsync(answeredQuestion);
        }

        public async Task UpdateAnsweredQuestions(List<AnsweredQuestion> answeredQuestions)
        {
            await _answeredQuestion.UpdateAnsweredQuestionsAsync(answeredQuestions);
        }
    }
}
