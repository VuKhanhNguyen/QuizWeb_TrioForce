using QuizWeb_TrioForce.Repositories.Interfaces;
using QuizWeb_TrioForce.Models;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels.Category;

namespace QuizWeb_TrioForce.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IFileService _fileService;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository, IFileService fileService)
        {
            _categoryRepository = categoryRepository;
            _fileService = fileService;
        }

        public async Task AddCategoryAsync(CategoryCreateViewModel viewModel)
        {
            var path = await _fileService.UploadFileAsync(viewModel.ImageFile);
            var cate = new Category
            {
                CategoryName = viewModel.CategoryName,
                ImgUrl = path!
            };  
            await _categoryRepository.AddCategoryAsync(cate);
        }
        public async Task UpdateCategoryAsync(Category category)
        {
            await _categoryRepository.UpdateCategoryAsync(category);
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var cate = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (cate != null)
            {
                await _categoryRepository.DeleteCategoryAsync(cate);
            }
        }

        public async Task<List<Category>> GetAllCategoryAsync()
        {
            return await _categoryRepository.GetAllCategoryAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            var cate = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (cate == null)
            {
                throw new Exception("Category not found");
            }
            return cate;

        }

    }
}
