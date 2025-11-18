using quizweb.Services.Interfaces;

namespace quizweb.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string?> UploadFileAsync(IFormFile file, string folder = "assets/categories")
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var allowedExtensions = new[] { ".png" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();  

            if (!allowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException("Only .PNG");
            }

            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, folder);
            if (!Directory.Exists(uploadPath)){
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/" + folder + "/" + fileName;
        }

        public Task DeleteFileAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
