using Microsoft.AspNetCore.Http;

namespace Backend_Mboagreen.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        public async Task<string> UploadImage(IFormFile file)
        {
            return "";
        }
    }
}