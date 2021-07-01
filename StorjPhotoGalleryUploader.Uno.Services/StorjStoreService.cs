using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Uno.Extensions;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.UnoHelpers.Contracts.Models;

namespace StorjPhotoGalleryUploader.Services
{
    public class StorjStoreService : IStoreService
    {
        readonly IBucketService _bucketService;
        readonly IUploadQueueService _uploadQueueService;

        public StorjStoreService(IBucketService bucketService, IUploadQueueService uploadQueueService)
        {
            _bucketService = bucketService;
            _uploadQueueService = uploadQueueService;
        }

        public async Task<bool> PutObjectAsync(AppConfig appConfig, Album album, string key, Stream objectData)
        {
            var bucket = await _bucketService.GetBucketAsync(appConfig.BucketName);

            try
            {
                await _uploadQueueService.AddObjectToUploadQueue(bucket.Name, key, appConfig.AccessGrant, objectData.ToMemoryStream().ToArray(), key);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
