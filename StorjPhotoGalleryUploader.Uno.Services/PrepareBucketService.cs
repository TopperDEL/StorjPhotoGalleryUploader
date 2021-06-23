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

namespace StorjPhotoGalleryUploader.Services
{
    public class PrepareBucketService : IPrepareBucketService
    {
        const string ASSET_BASE = "StorjPhotoGalleryUploader.Services.Assets.site_template.";
        public event PreparationStateChangedEventHandler PreparationStateChangedEvent;

        private IEnumerable<string> _assetNames;

        readonly IObjectService _objectService;
        readonly IBucketService _bucketService;
        readonly AppConfig _appConfig;
        private Bucket _currentBucket;

        public PrepareBucketService(IObjectService objectService, IBucketService bucketService, AppConfig appConfig)
        {
            _objectService = objectService;
            _bucketService = bucketService;
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
                    _currentBucket = await _bucketService.GetBucketAsync(_appConfig.BucketName);
                }
                catch
                {
                    try
                    {
                        _currentBucket = await _bucketService.EnsureBucketAsync(_appConfig.BucketName);
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
                var content = await _objectService.ListObjectsAsync(_currentBucket, listOptions);

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

                int current = 0;
                foreach (var name in _assetNames)
                {
                    var fileName = GetFileName(name);
                    PreparationStateChangedEvent?.Invoke(current, _assetNames.Count(), fileName);

                    using (var stream = assembly.GetManifestResourceStream(name))
                    {
                        var upload = await _objectService.UploadObjectAsync(_currentBucket, fileName, new UploadOptions(), stream, false);
                        await upload.StartUploadAsync();

                        if (!upload.Completed)
                        {
                            return new BucketPrepareResult() { Successfull = false, PrepareErrorMessage = "Could not upload file '" + fileName + "'" };
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
