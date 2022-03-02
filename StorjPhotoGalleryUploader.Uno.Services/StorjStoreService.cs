using MonkeyCache.FileStore;
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
using uplink.NET.UnoHelpers.Services;

namespace StorjPhotoGalleryUploader.Services
{
    public class StorjStoreService : IStoreService
    {
        readonly IBucketService _bucketService;
        readonly IObjectService _objectService;
        readonly IUploadQueueService _uploadQueueService;

        public StorjStoreService(IBucketService bucketService, IObjectService objectService, IUploadQueueService uploadQueueService)
        {
            _bucketService = bucketService;
            _objectService = objectService;
            _uploadQueueService = uploadQueueService;
        }

        public async Task<Stream> GetObjectAsStreamAsync(AppConfig appConfig, string key)
        {
            if (!Barrel.Current.IsExpired(key))
            {
                return new MemoryStream(Barrel.Current.Get<byte[]>(key));
            }
            try
            {
                var bucket = await _bucketService.GetBucketAsync(appConfig.BucketName);
                var objectInfo = await _objectService.GetObjectAsync(bucket, key);
                var downloadStream = new CachingDownloadStream(bucket, (int)objectInfo.SystemMetadata.ContentLength, key);
                downloadStream.DownloadStreamCachedEvent += (sender, data) => { Barrel.Current.Add(key, data, TimeSpan.FromDays(365)); };
                return downloadStream;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> PutObjectAsync(AppConfig appConfig, string key, Stream objectData, string identifier)
        {
            try
            {
                if (!key.Contains("original"))
                {
                    if (objectData is MemoryStream)
                    {
                        MemoryStream mstream = objectData as MemoryStream;
                        Barrel.Current.Add(key, mstream.ToArray(), TimeSpan.FromDays(365));
                        mstream.Position = 0;
                    }
                }

                var accessGrant = appConfig.TryGetAccessGrant(out bool success);
                if (!success)
                    return false;
                await _uploadQueueService.CancelUploadAsync(key); //Remove previous upload-request - the new one is the one to go
                if (objectData == null)
                {
                    return false;
                }
                await _uploadQueueService.AddObjectToUploadQueueAsync(appConfig.BucketName, key, accessGrant, objectData, identifier).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
