using quizweb.Models;
using quizweb.Repositories.Interfaces;
using quizweb.Services.Interfaces;
using quizweb.ViewModels.BookMark;

namespace quizweb.Services.Implementations
{
    public class QuestionSetService : IQuestionSetService
    {
        private readonly IQuestionSetRepository _questionSetRepository;

        public QuestionSetService(IQuestionSetRepository questionSetRepository)
        {
            _questionSetRepository = questionSetRepository;
        }

        public async Task<QuestionSet> GetQuestionSetRandomByIdCateAndIdLevel(int idCate, int idLevel)
        {
            var qs = await _questionSetRepository.GetQuestionSetRandomByIdCateAndIdLevel(idCate, idLevel);

            return qs ?? throw new Exception("QuestionSetRandom is not found"); ;
        }

        public async Task<List<CreatedQuestionSetListViewModel>> GetAllCreatedQuestionSetsAsync(string username)
        {
            var cqsList = await _questionSetRepository.GetAllCreatedQuestionSetsByUsernameAsync(username);
            return cqsList.Select(cqs => new CreatedQuestionSetListViewModel()
            {
                QSetId = cqs.QSetId,
                QSetName = cqs.QSetName,
                CreatedTime = cqs.CreatedTime,
                Description = cqs.Description,
                CategoryId = cqs.CategoryId,
                LevelId = cqs.LevelId
            }).ToList();
        }

        public async Task<QuestionSet> GetQuestionSetByIdAsync(int id)
        {
            var qs = await _questionSetRepository.GetQuestionSetByIdAsync(id);
            
            return qs ?? throw new Exception("QuestionSetRandom is not found");
        }
    }
}
