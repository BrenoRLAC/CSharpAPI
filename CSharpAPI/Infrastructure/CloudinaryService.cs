using API.Utilities;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CloudinaryServiceInterface.Infrastructure;

namespace CloudinaryServices.Infrastructure
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {

            var cloudName = configuration["Cloudinary:cloud_name"];
            var apiKey = configuration["Cloudinary:api_key"];
            var apiSecret = configuration["Cloudinary:api_secret"];


            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);

        }

        public ImageUploadResult UploadImages(IFormFile file)
        {
            var result = new ImageUploadResult();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream())
            };

            var uploadResult = _cloudinary.Upload(uploadParams);


            if (!string.IsNullOrEmpty(uploadResult.Error?.Message))
            {
                throw new Exception(uploadResult.Error.Message);
            }

            result.PublicId = uploadResult.PublicId;
            result.SecureUrl = uploadResult.SecureUrl;

            return result;
        }

        public async Task<DeletionResult> DeleteImage(string imageId)
        {
            var deletionParams = new DeletionParams(imageId.Decrypt());
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);
            return deletionResult;

        }
    }
}
