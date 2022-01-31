using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.UnoHelpers.Contracts.Models;
using uplink.NET.UnoHelpers.Services;

namespace StorjPhotoGalleryUploader.Services
{
    public class PrepareBucketService : IPrepareBucketService
    {
        const string ASSET_BASE = "StorjPhotoGalleryUploader.Services.Assets.site_template.";
        public event PreparationStateChangedEventHandler PreparationStateChangedEvent;

        private IEnumerable<string> _assetNames;

        readonly IObjectService _objectService;
        readonly IBucketService _bucketService;
        readonly IUploadQueueService _uploadQueueService;
        readonly AppConfig _appConfig;
        private Bucket _currentBucket;

        public PrepareBucketService(IObjectService objectService, IBucketService bucketService, IUploadQueueService uploadQueueService, AppConfig appConfig)
        {
            _objectService = objectService;
            _bucketService = bucketService;
            _uploadQueueService = uploadQueueService;
            _appConfig = appConfig;
        }

        private async Task InitAsync()
        {
            if (_assetNames == null)
            {
                Assembly assembly = GetType().GetTypeInfo().Assembly;
                _assetNames = assembly.GetManifestResourceNames().Where(n => n.Contains("StorjPhotoGalleryUploader.Services.Assets.site_template") && !n.Contains("index.html"));
            }
            if (_currentBucket == null)
            {
                try
                {
                    _currentBucket = await _bucketService.GetBucketAsync(_appConfig.BucketName).ConfigureAwait(false);
                }
                catch
                {
                    try
                    {
                        _currentBucket = await _bucketService.EnsureBucketAsync(_appConfig.BucketName).ConfigureAwait(false);
                    }
                    catch
                    {
                        //ToDo: info to user
                    }
                }
            }
        }

        public async Task<bool> CheckIfBucketIsReadyAsync()
        {
            await InitAsync();

            try
            {
                var listOptions = new ListObjectsOptions();
                listOptions.Recursive = true;
                listOptions.Prefix = "assets/";
                var content = await _objectService.ListObjectsAsync(_currentBucket, listOptions).ConfigureAwait(false);

                foreach (var necessaryFile in _assetNames)
                {
                    if (content.Items.Where(i => i.Key.EndsWith(GetFileName(necessaryFile))).Count() != 1)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                //Whatever happened, try to prepare again (which would throw additional infos then)
                return false;
            }
        }

        public async Task<BucketPrepareResult> PrepareBucketAsync()
        {
            try
            {
                await InitAsync();

                Assembly assembly = GetType().GetTypeInfo().Assembly;

                var accessGrant = _appConfig.TryGetAccessGrant(out bool success);
                if (!success)
                    return new BucketPrepareResult() { Successfull = false, PrepareErrorMessage = "Wrong ApiKey" };

                var currentUploads = await _uploadQueueService.GetAwaitingUploadsAsync();
                int current = 0;
                foreach (var name in _assetNames)
                {
                    var fileName = GetFileName(name);
                    PreparationStateChangedEvent?.Invoke(current, _assetNames.Count(), fileName);

                    using (var stream = assembly.GetManifestResourceStream(name))
                    {
                        if (currentUploads.Where(u => u.AccessGrant == accessGrant && u.Key == fileName && u.BucketName == _currentBucket.Name).Count() == 0)
                        {
                            //File is not yet in upload queue for this access => upload it
                            var identifier = fileName.Split('/').Last();
                            await _uploadQueueService.AddObjectToUploadQueueAsync(_currentBucket.Name, fileName, accessGrant, stream, identifier);
                        }
                    }

                    current++;
                }

                return new BucketPrepareResult() { Successfull = true };

            }
            catch (Exception ex)
            {
                return new BucketPrepareResult() { Successfull = false, PrepareErrorMessage = ex.Message };
            }
        }

        private string GetFileName(string assetName)
        {
            var fileName = assetName.Replace(ASSET_BASE, "");
            while (fileName.Where(c => c == '.').Count() > 1)
            {
                //Some names have two dots, like "awesome-library.min.js"
                if (fileName.Where(c => c == '.').Count() == 2 && (fileName.Contains(".min.") || fileName.Contains(".lazyload.")))
                {
                    break;
                }
                var index = fileName.IndexOf('.');
                StringBuilder sb = new StringBuilder(fileName);
                sb[index] = '/';
                fileName = sb.ToString();
            }

            //The order in the template is different, so flip it
            fileName = fileName.Replace("album/assets/", "assets/album/");
            fileName = fileName.Replace("homepage/assets/", "assets/homepage/");
            return fileName;
        }
    }
}
