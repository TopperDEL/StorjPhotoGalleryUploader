using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.UnoHelpers.Contracts.Models;

namespace StorjPhotoGalleryUploader.Services
{
    public class StorjStoreService : IStoreService
    {
        readonly IObjectService _objectService;
        readonly IBucketService _bucketService;

        public StorjStoreService(IObjectService objectService, IBucketService bucketService)
        {
            _objectService = objectService;
            _bucketService = bucketService;
        }

        public async Task<bool> PutObjectAsync(AppConfig appConfig, Album album, string key, Stream objectData)
        {
            var bucket = await _bucketService.GetBucketAsync(appConfig.BucketName);

            var upload = await _objectService.UploadObjectAsync(bucket, key, new UploadOptions(), objectData, false);
            upload.UploadOperationProgressChanged += (prog) =>
            {
                System.Diagnostics.Debug.WriteLine("Uploaded: " + prog.PercentageCompleted + "%");
            };
            await upload.StartUploadAsync();

            return upload.Completed;
        }
    }
}
