namespace QuizWeb_TrioForce.Services.Interfaces
{
    public interface IFileService
    {
        Task<string?> UploadFileAsync(IFormFile file, string folder = "assets/categories");
        Task DeleteFileAsync(string filePath);
    }
}
