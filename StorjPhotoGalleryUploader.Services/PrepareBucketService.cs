using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using uplink.NET.Interfaces;

namespace StorjPhotoGalleryUploader.Services
{
    public class PrepareBucketService : IPrepareBucketService
    {
        public event PreparationStateChangedEventHandler PreparationStateChangedEvent;

        private IEnumerable<string> _assetNames;

        readonly IObjectService _objectService;
        readonly IBucketService _bucketService;

        public PrepareBucketService(IObjectService objectService, IBucketService bucketService)
        {
            _objectService = objectService;
            _bucketService = bucketService;
        }

        private void Init()
        {
            if (_assetNames == null)
            {
                Assembly assembly = GetType().GetTypeInfo().Assembly;
                _assetNames = assembly.GetManifestResourceNames().Where(n => n.Contains("StorjPhotoGalleryUploader.Services.Assets.site_template") && !n.Contains("index.html"));
            }
        }

        public async Task<bool> CheckIfBucketNeedsPrepareAsync()
        {
            Init();

            //ToDo: Check if every file exists in the bucket
            await Task.Delay(100);
            return false;
        }

        public async Task<BucketPrepareResult> PrepareBucketAsync()
        {
            Init();   

            int current = 0;
            foreach(var name in _assetNames)
            {
                var fileName = name.Replace("StorjPhotoGalleryUploader.Services.Assets.site_template.", "");
                PreparationStateChangedEvent?.Invoke(current, _assetNames.Count(), fileName);
                await Task.Delay(10); //ToDo: Upload file, respect correct filename
                current++;
            }

            return new BucketPrepareResult() { Successfull = false, PrepareErrorMessage = "This is a long and longer info about what might have happened. So show it to the user then..." };
        }
    }
}
