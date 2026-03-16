using CloudinaryDotNet.Actions;

namespace CloudinaryServiceInterface.Infrastructure
{
    public interface ICloudinaryService
    {
        ImageUploadResult UploadImages(IFormFile filePath);
        Task<DeletionResult> DeleteImage(string imageId);
    }
}