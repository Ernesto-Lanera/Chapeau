using Microsoft.AspNetCore.Http;
namespace Chapeau.Services
{
    public class ImageService(IWebHostEnvironment webHostEnvironment, ILogger<ImageService> logger) : IImageService
    {
        private const string UploadFolderName = "images/menu-items";
        private const long MaxFileSize = 10 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly ILogger<ImageService> _logger = logger;

        public async Task<(bool Success, string? Path, string? ErrorMessage)> UploadImageAsync(IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                return (true, null, null);
            }

            if (file.Length > MaxFileSize)
            {
                return (false, null, "Bestand is te groot. Maximum 10 MB.");
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return (false, null, "Alleen JPG-, PNG- en WebP-bestanden zijn toegestaan.");
            }

            try
            {
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, UploadFolderName);
                Directory.CreateDirectory(uploadPath);

                string filename = $"{Guid.NewGuid():N}{extension}";
                string filePath = Path.Combine(uploadPath, filename);
                await using FileStream stream = new(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                return (true, $"/{UploadFolderName}/{filename}", null);
            }
            catch (IOException exception)
            {
                _logger.LogError(exception, "Afbeelding opslaan is mislukt.");
                return (false, null, "Afbeelding kon niet worden opgeslagen.");
            }
        }
    }
}
