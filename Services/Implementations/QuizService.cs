using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public async Task DeleteQuizAsync(int id, string username)
        {

            var qs = await _unitOfWork.QuestionSetRepository.GetQuestionSetByIdAsync(id);
            if (qs == null)
            {
                return;
            }
            if (!string.Equals(qs.AuthorName, username, StringComparison.Ordinal))
                return;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var answers = await _unitOfWork.AnswerRepository.GetAllAnswersByQSetIdAsync(id);
                _unitOfWork.AnswerRepository.DeleteAnswersAsync(answers);

                var questions = await _unitOfWork.QuestionRepository.GetAllQuestionsByIdQSetAsync(id);
                _unitOfWork.QuestionRepository.DeleteQuestionsAsync(questions);
                
                _unitOfWork.QuestionSetRepository.DeleteQuestionSet(qs);
                
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
            var viewModel = MapToPlay(qs);

            return viewModel;
        }

        public async Task<PlayQuestionSetViewModel> GetRandomQuizAsync()
        {
            var qs = await _unitOfWork.QuestionSetRepository.GetQuestionSetRandomByNewGuidAsync();
            if (qs == null)
            {
                throw new Exception("GetQuestionSetRandomByNewGuid is not found");
            }

            var viewModel = MapToPlay(qs);
            return viewModel;
        }

        public async Task<PlayQuestionSetViewModel> GetQuizByCategoryAndLevelAsync(int categoryId, int levelId)
        {
            var qs = await _unitOfWork.QuestionSetRepository.GetQuestionSetRandomByIdCateAndIdLevel(categoryId, levelId);
            if (qs == null)
            {
                throw new Exception("GetQuestionSetRandomByIdCateAndIdLevel is not found");
            }

            var viewModel = MapToPlay(qs);
            return viewModel;
        }


        private PlayQuestionSetViewModel MapToPlay(QuestionSet qs)
        {
            return new PlayQuestionSetViewModel
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

            // Get full question set with all answers for detailed result
            var questionSet = await _unitOfWork.QuestionSetRepository.GetQuestionSetByIdAsync(submitModel.QSetId);
            if (questionSet == null)
            {
                throw new Exception($"Question set with ID {submitModel.QSetId} not found");
            }

            var correctAnswersDict = listQAnswer.ToDictionary(key => key.QuestionId, values => values.CorrectAnswerIds);
            int score = 0;
            var questionsResult = new List<QuestionResultViewModel>(submitModel.UserAnswers.Count);
            var progQuesSet = new ProgressQuestionSetViewModel
            {
                QSetId = submitModel.QSetId,
                QuestionCount = submitModel.QuestionCount,
                QuestionLastId = submitModel.QuestionLastId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };

            // Create dictionary of user answers for quick lookup
            var userAnswersDict = submitModel.UserAnswers.ToDictionary(ua => ua.QuestionId, ua => ua.SelectedAnswerId);

            foreach (var question in questionSet.Questions)
            {
                bool isCorrect = false;
                int correctAnswerId = 0;
                string correctAnswerText = string.Empty;
                int userSelectedAnswerId = 0;
                string userSelectedAnswerText;

                // Get correct answer info
                if (correctAnswersDict.TryGetValue(question.QuestionId, out var correctAnswerIdSet))
                {
                    correctAnswerId = correctAnswerIdSet.FirstOrDefault();
                    var correctAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == correctAnswerId);
                    correctAnswerText = correctAnswer?.AnswerText ?? "N/A";
                }

                // Check if user answered this question
                if (userAnswersDict.TryGetValue(question.QuestionId, out var selectedAnswerId))
                {
                    userSelectedAnswerId = selectedAnswerId;
                    var userAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == selectedAnswerId);
                    userSelectedAnswerText = userAnswer?.AnswerText ?? "Không có đáp án";
                    
                    if (correctAnswerIdSet != null && correctAnswerIdSet.Contains(selectedAnswerId))
                    {
                        isCorrect = true;
                        score += 10;
                    }

                }
                else
                {
                    userSelectedAnswerText = "Không trả lời";
                }

                // Build all answers list
                var allAnswers = question.Answers.Select(a => new AnswerOptionViewModel
                {
                    AnswerId = a.AnswerId,
                    AnswerText = a.AnswerText,
                    IsCorrect = correctAnswerIdSet != null && correctAnswerIdSet.Contains(a.AnswerId),
                    IsUserSelected = a.AnswerId == userSelectedAnswerId
                }).ToList();

                // Build result with full details
                questionsResult.Add(new QuestionResultViewModel
                {
                    QuestionId = question.QuestionId,
                    QuestionText = question.QuestionText,
                    UserSelectedAnswerId = userSelectedAnswerId,
                    UserSelectedAnswerText = userSelectedAnswerText,
                    CorrectAnswerId = correctAnswerId,

                    CorrectAnswerText = correctAnswerText,
                    IsCorrect = isCorrect,
                    AllAnswers = allAnswers
                });
            }

            await _answeredQuestionService.SaveAnsweredQuestions(username, submitModel.QSetId, submitModel.UserAnswers);

            var progressQuestionSetExist = await _progressQuestionSetService.GetProgressQuestionSetByUsernameAndQSetId(username, submitModel.QSetId);
            if (progressQuestionSetExist != null)
            {
                await _progressQuestionSetService.UpdateProgressQuestionSet(progQuesSet, username);
            }
            else
            {
                await _progressQuestionSetService.AddProgressQuestionSet(progQuesSet, username);
            }

            await _rankingService.UpdateUserScoreAsync(username, score);

            return new QuizResultViewModel
            {
                QSetId = submitModel.QSetId,
                QSetName = questionSet.QSetName,
                TotalQuestions = questionSet.Questions.Count,
                Score = score,
                QuestionResults = questionsResult
            };

        }

        public Task UpdateQuizAsync(UpdateQuestionSetViewModel viewModel, string authorName)
        {
            throw new NotImplementedException();
        }
        public async Task<ResumeQuestionSetViewModel> LoadProgressAsync(string username, int qSetId)
        {
            var progress = await _progressQuestionSetService.GetProgressQuestionSetByUsernameAndQSetId(username, qSetId) ?? throw new Exception($"Progress for user {username} and quiz {qSetId} not found");
            var answeredQuestions = await _answeredQuestionService.GetAllAnsweredQuestions(username, qSetId) ?? throw new Exception($"Answered questions for user {username} and quiz {qSetId} not found");
            var questionSet = await _unitOfWork.QuestionSetRepository.GetQuestionSetByIdAsync(qSetId) ?? throw new Exception($"Quiz with ID {qSetId} not found");

            var answeredDict = answeredQuestions.ToDictionary(aq => aq.QuestionId, aq => aq.SelectedAnswerId);

            var preViewModel = MapToPlay(questionSet);

            ResumeQuestionSetViewModel viewModel = new ResumeQuestionSetViewModel()
            {
                QSetId = preViewModel.QSetId,
                QSetName = preViewModel.QSetName,
                Description = preViewModel.Description,
                AuthorName = preViewModel.AuthorName,
                CategoryName = preViewModel.CategoryName,
                LevelName = preViewModel.LevelName,

                Questions = preViewModel.Questions,

                QuestionLastId = progress.QuestionLastId,
                LastUpdated = progress.LastUpdated,
            };

            viewModel.Questions.ForEach(q =>
            {
                if (answeredDict.TryGetValue(q.QuestionId, out int selectedId))
                {
                    q.UserSelectedAnswerId = selectedId;
                }
                else
                {
                    q.UserSelectedAnswerId = null;
                }
            });

            return viewModel;
        }

    }
}
