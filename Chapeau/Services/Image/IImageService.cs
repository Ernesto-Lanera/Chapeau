using Microsoft.AspNetCore.Http;
namespace Chapeau.Services
{
    public interface IImageService
    {
        Task<(bool Success, string? Path, string? ErrorMessage)> UploadImageAsync(IFormFile? file);
    }
}
