using QuizWeb_TrioForce.DTOs;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Repositories.Interfaces;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.Answer;
using QuizWeb_TrioForce.ViewModels.ProgressQuestionSet;
using QuizWeb_TrioForce.ViewModels.Question;
using QuizWeb_TrioForce.ViewModels.QuestionSet;

namespace QuizWeb_TrioForce.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRankingService _rankingService;
        private readonly IAnsweredQuestionService _answeredQuestionService;
        private readonly IProgressQuestionSetService _progressQuestionSetService;

        public QuizService(IUnitOfWork unitOfWork, IRankingService rankingService, IAnsweredQuestionService answeredQuestionService, IProgressQuestionSetService progressQuestionSetService)
        {
            _unitOfWork = unitOfWork;
            _rankingService = rankingService;
            _answeredQuestionService = answeredQuestionService;
            _progressQuestionSetService = progressQuestionSetService;
        }

        public async Task CreateQuizAsync(CreateQuestionSetViewModel viewModel, string authorName)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var qs = new QuestionSet
                {
                    QSetName = viewModel.QSetName,
                    Description = viewModel.Description,
                    AuthorName = authorName,
                    LevelId = viewModel.LevelId,
                    CategoryId = viewModel.CategoryId,
                    CreatedTime = DateTime.UtcNow,
                };
                await _unitOfWork.QuestionSetRepository.AddQuestionSetAsync(qs);
                await _unitOfWork.SaveChangesAsync();

                //old approach:

                //foreach (var q in viewModel.Questions)
                //{
                //    var question = new Question
                //    {
                //        QuestionText = q.QuestionText,
                //        QSetId = qs.QSetId,
                //    };
                //    await _unitOfWork.QuestionRepository.AddQuestionAsync(question);
                //    await _unitOfWork.SaveChangesAsync();

                //    foreach (var a in q.Answers)
                //    {
                //        var answer = new Answer
                //        {
                //            QuestionId = question.QuestionId,
                //            AnswerText = a.AnswerText,
                //            IsCorrect = a.IsCorrect,

                //        };
                //        await _unitOfWork.AnswerRepository.AddAnswerAsync(answer);
                //    }

                //}


                //new approach:

                var questionsList = viewModel.Questions.Select(q => new Question
                {
                    QuestionText = q.QuestionText,
                    QSetId = qs.QSetId
                }).ToList();

                await _unitOfWork.QuestionRepository.AddQuestionsAsync(questionsList);
                await _unitOfWork.SaveChangesAsync();

                var allAnswers = new List<Answer>();

                for (var i = 0; i < viewModel.Questions.Count; i++)
                {
                    var idQuestionCreated = questionsList[i].QuestionId;

                    var answers = viewModel.Questions[i].Answers.Select(a => new Answer()
                    {
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect,
                        QuestionId = idQuestionCreated
                    }).ToList();

                    allAnswers.AddRange(answers);
                }

                await _unitOfWork.AnswerRepository.AddAnswersAsync(allAnswers);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

            }
            catch (Exception)
            {

                await _unitOfWork.RollbackAsync();
                throw;
            }

        }

        public async Task DeleteQuizAsync(int idQset)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var answers = await _unitOfWork.AnswerRepository.GetAllAnswersByQSetIdAsync(idQset);
                _unitOfWork.AnswerRepository.DeleteAnswersAsync(answers);

                var questions = await _unitOfWork.QuestionRepository.GetAllQuestionsByIdQSetAsync(idQset);
                _unitOfWork.QuestionRepository.DeleteQuestionsAsync(questions);

                var questionSet = await _unitOfWork.QuestionSetRepository.GetQuestionSetByIdAsync(idQset);
                if (questionSet != null)
                {
                    _unitOfWork.QuestionSetRepository.DeleteQuestionSetAsync(questionSet);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

            }
            catch (Exception)
            {

                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<PlayQuestionSetViewModel> GetQuizAsync(int id)
        {
            var qs = await _unitOfWork.QuestionSetRepository.GetQuestionSetByIdAsync(id);
            if (qs == null)
            {
                throw new Exception($"Quiz with ID {id} not found");
            }

            var viewModel = new PlayQuestionSetViewModel
            {
                QSetId = qs.QSetId,
                QSetName = qs.QSetName,
                Description = qs.Description,
                AuthorName = qs.AuthorName,
                CategoryName = qs.Category.CategoryName,
                LevelName = qs.Level.LevelName,
                Questions = qs.Questions.Select(q => new PlayQuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Answers = q.Answers.Select(a => new PlayAnswerViewModel
                    {
                        AnswerId = a.AnswerId,
                        AnswerText = a.AnswerText,
                    }).ToList()
                }).ToList()
            };

            return viewModel;
        }

        public async Task<PlayQuestionSetViewModel> GetRandomQuizAsync()
        {
            var qs = await _unitOfWork.QuestionSetRepository.GetQuestionSetRandomByNewGuidAsync();
            if (qs == null)
            {
                throw new Exception("GetQuestionSetRandomByNewGuid is not found");
            }
            var viewModel = new PlayQuestionSetViewModel
            {
                QSetId = qs.QSetId,
                QSetName = qs.QSetName,
                Description = qs.Description,
                AuthorName = qs.AuthorName,
                CategoryName = qs.Category.CategoryName,
                LevelName = qs.Level.LevelName,
                Questions = qs.Questions.Select(q => new PlayQuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Answers = q.Answers.Select(a => new PlayAnswerViewModel
                    {
                        AnswerId = a.AnswerId,
                        AnswerText = a.AnswerText,
                    }).ToList()
                }).ToList()

            };
            return viewModel;
        }

        public async Task SaveProgressAsync(SaveProgressViewModel saveModel, string username)
        {

            var progressQuestionSet = new ProgressQuestionSetViewModel
            {
                QSetId = saveModel.QSetId,
                QuestionCount = saveModel.QuestionCount,
                QuestionLastId = saveModel.QuestionLastId
            };

            var progressQuestionSetExist = await _progressQuestionSetService.GetProgressQuestionSetByUsernameAndQSetId(username, saveModel.QSetId);
            if (progressQuestionSetExist != null)
            {
                await _progressQuestionSetService.UpdateProgressQuestionSet(progressQuestionSet, username);
            }
            else
            {
                await _progressQuestionSetService.AddProgressQuestionSet(progressQuestionSet, username);
            }

            await _answeredQuestionService.SaveAnsweredQuestions(username, saveModel.QSetId, saveModel.UserAnswers);

        }

        public async Task<QuizResultViewModel> SubmitQuizAsync(SubmitQuizViewModel submitModel, string username)
        {

            if (submitModel == null)
            {
                throw new ArgumentNullException(nameof(submitModel));
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required", nameof(username));
            }
            if (submitModel?.UserAnswers == null || !submitModel.UserAnswers.Any())
            {
                throw new ArgumentException("No answers provided");
            }

            var listQAnswer = await _unitOfWork.QuestionSetRepository.GetCorrectAnswerSetByIdAsync(submitModel.QSetId);

            if (listQAnswer == null)
            {
                throw new Exception($"Quiz with ID {submitModel.QSetId} not found");
            }

            var correctAnswersDict = listQAnswer.ToDictionary(key => key.QuestionId, values => values.CorrectAnswerIds);
            int score = 0;
            var questionsResult = new List<QuestionResultViewModel>(submitModel.UserAnswers.Count);
            var answeredQuestions = new List<AnsweredQuestion>(submitModel.UserAnswers.Count);
            var progQuesSet = new ProgressQuestionSetViewModel
            {
                QSetId = submitModel.QSetId,
                QuestionCount = submitModel.QuestionCount,
                QuestionLastId = submitModel.QuestionLastId
            };

            foreach (var question in submitModel.UserAnswers)
            {
                bool isCorrect = false;
                int correctAnswerId = 0;

                if (correctAnswersDict.TryGetValue(question.QuestionId, out var correctAnswerIdSet))
                {

                    correctAnswerId = correctAnswerIdSet.FirstOrDefault(); //get first correct answer id for useranswer incorrect case
                    isCorrect = correctAnswerIdSet.Contains(question.SelectedAnswerId);

                    if (isCorrect)
                    {
                        score++;
                    }
                }


                questionsResult.Add(new QuestionResultViewModel
                {
                    QuestionId = question.QuestionId,
                    UserSelecteAnswerId = question.SelectedAnswerId,
                    CorrectAnswerId = correctAnswerId,
                    IsCorrect = isCorrect
                });

                answeredQuestions.Add(new AnsweredQuestion
                {
                    UserName = username,
                    QSetId = submitModel.QSetId,
                    QuestionId = question.QuestionId,
                    SelectedAnswerId = question.SelectedAnswerId
                });
            }

            await _progressQuestionSetService.AddProgressQuestionSet(progQuesSet, username);
            await _answeredQuestionService.AddAnsweredQuestions(answeredQuestions);
            await _rankingService.UpdateUserScoreAsync(username, score);

            return new QuizResultViewModel
            {
                QSetId = submitModel.QSetId,
                QSetName = submitModel.QSetName,
                TotalQuestions = listQAnswer.Count(),
                Score = score,
                QuestionResults = questionsResult
            };
            
        }

        public Task UpdateQuizAsync(UpdateQuestionSetViewModel viewModel, string authorName)
        {
            throw new NotImplementedException();
        }

    }
}
