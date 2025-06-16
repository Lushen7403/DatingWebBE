using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace MatchLoveWeb.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using Microsoft.AspNetCore.Http;

    public class PhotoUploadResult
    {
        public string Url { get; set; }
        public string PublicId { get; set; }
    }

    public class PhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        /// <summary>
        /// Upload a single avatar image to the "Avatars" folder.
        /// </summary>
        public async Task<PhotoUploadResult> UploadAvatarAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "Avatars"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return new PhotoUploadResult
            {
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId
            };
        }

        /// <summary>
        /// Upload multiple profile images to the "ProfileImage" folder.
        /// </summary>
        public async Task<IEnumerable<PhotoUploadResult>> UploadProfileImagesAsync(List<IFormFile> files)
        {
            var uploadResults = new List<PhotoUploadResult>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "ProfileImage"
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                uploadResults.Add(new PhotoUploadResult
                {
                    Url = result.SecureUrl.ToString(),
                    PublicId = result.PublicId
                });
            }

            return uploadResults;
        }

        /// <summary>
        /// Delete an image by its public ID.
        /// </summary>
        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }

        public async Task<PhotoUploadResult> UploadMediaAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "MessageMedia"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return new PhotoUploadResult
            {
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId
            };
        }

    }


}
