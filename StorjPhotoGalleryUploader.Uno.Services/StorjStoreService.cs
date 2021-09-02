﻿using StorjPhotoGalleryUploader.Contracts.Interfaces;
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
using uplink.NET.UnoHelpers.Services;

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

        public async Task<bool> PutObjectAsync(AppConfig appConfig, Album album, string key, Stream objectData, string identifier)
        {
            var bucket = await _bucketService.GetBucketAsync(appConfig.BucketName).ConfigureAwait(false);

            try
            {
                var accessGrant = appConfig.TryGetAccessGrant(out bool success);
                if (!success)
                    return false;

                await _uploadQueueService.AddObjectToUploadQueueAsync(bucket.Name, key, accessGrant, objectData, identifier).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
