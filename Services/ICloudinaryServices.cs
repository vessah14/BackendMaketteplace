using Microsoft.AspNetCore.Http;

namespace Backend_Mboagreen.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImage(IFormFile file);
    }
}