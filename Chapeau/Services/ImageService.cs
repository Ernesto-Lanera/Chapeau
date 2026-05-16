using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Chapeau.Services
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string UploadFolderName = "images/menu-items";
        private const long MaxFileSize = 10 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<(bool Success, string? Path, string? ErrorMessage)> UploadImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (true, null, null);

            if (file.Length > MaxFileSize)
                return (false, null, "Bestand is te groot. Maximum 10MB.");

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!Array.Exists(AllowedExtensions, element => element == fileExtension))
                return (false, null, "Alleen JPG, PNG en WebP bestanden zijn toegestaan.");

            try
            {
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, UploadFolderName);
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/{UploadFolderName}/{fileName}";
                return (true, relativePath, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Fout bij upload: {ex.Message}");
            }
        }
    }
}