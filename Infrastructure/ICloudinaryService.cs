using API.Domain.HeroImages;
using CloudinaryDotNet.Actions;

namespace CloudinaryServiceInterface.Infrastructure
{
    public interface ICloudinaryService
    {
        List<ImageUploadResult> UploadImages(List<IFormFile> filePath);
        Task<DeletionResult> DeleteImage(string imageId);
    }
}