using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.UnoHelpers.Contracts.Interfaces;
using uplink.NET.UnoHelpers.Contracts.Models;

namespace StorjPhotoGalleryUploader.Services
{
    public class PhotoUploadService : IPhotoUploadService
    {
        private readonly IThumbnailGeneratorService _thumbnailGeneratorService;
        private readonly IStoreService _storeService;
        private readonly AppConfig _appConfig;

        public PhotoUploadService(AppConfig appConfig, IThumbnailGeneratorService thumbnailGeneratorService, IStoreService storeService)
        {
            _thumbnailGeneratorService = thumbnailGeneratorService;
            _storeService = storeService;
            _appConfig = appConfig;
        }

        public async Task CreateAndUploadAsync(string albumName, string fileName, Stream imageStream, ImageResolution resolution)
        {
            string identifier;
            Stream streamToUse;
            if (resolution.Value == ImageResolution.Small)
            {
                streamToUse = await _thumbnailGeneratorService.GenerateThumbnailForStreamAsync(imageStream, "image/jpeg", 360, 225);
                identifier = ImageResolution.SmallDescription + "/" + fileName + " - " + albumName;
            }
            else if (resolution.Value == ImageResolution.Medium)
            {
                streamToUse = await _thumbnailGeneratorService.GenerateThumbnailForStreamAsync(imageStream, "image/jpeg", 1200, 750);
                identifier = ImageResolution.MediumDescription + "/" + fileName + " - " + albumName;
            }
            else if (resolution.Value == ImageResolution.Original)
            {
                streamToUse = imageStream;
                identifier = fileName + " - " + albumName;
            }
            else
            {
                return;
            }

            await _storeService.PutObjectAsync(_appConfig, "pics/" + resolution + "/" + albumName + "/" + fileName, streamToUse, identifier);
        }

        public async Task CreateAndUploadCoverImageAsync(string albumName, string fileName, Stream imageStream, ImageResolution resolution)
        {
            var streamToUse = await _thumbnailGeneratorService.GenerateThumbnailForStreamAsync(imageStream, "image/jpeg", 1200, 750);
            await _storeService.PutObjectAsync(_appConfig, "pics/" + ImageResolution.Medium + "/" + albumName + "/cover_image.jpg", streamToUse, "Cover-Image");
        }
    }
}
