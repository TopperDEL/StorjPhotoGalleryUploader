using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using System.Linq;

namespace StorjPhotoGalleryUploader.Services
{
    public class AlbumService : IAlbumService
    {
        readonly IBucketService _bucketService;
        readonly IObjectService _objectService;
        readonly AppConfig _appConfig;
        private Bucket _bucket;

        public AlbumService(IBucketService bucketService, IObjectService objectService, AppConfig appConfig)
        {
            _bucketService = bucketService;
            _objectService = objectService;
            _appConfig = appConfig;
        }

        private async Task InitAsync()
        {
            if(_bucket == null)
            {
                try
                {
                    _bucket = await _bucketService.EnsureBucketAsync(_appConfig.BucketName);
                }
                catch (Exception ex2) {
                    try
                    {
                        _bucket = await _bucketService.GetBucketAsync(_appConfig.BucketName);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public async Task<Album> CreateAlbumAsync(string albumName)
        {
            return new Album() { Name = albumName }; //ToDo
        }

        public async Task<List<Album>> ListAlbumsAsync()
        {
            List<Album> albums = new List<Album>();

            await InitAsync();
            if (_bucket == null)
                return albums;

            ListObjectsOptions listOptions = new ListObjectsOptions();
            listOptions.Recursive = true;
            listOptions.Prefix = "pics/original/";
            var albumItems = await _objectService.ListObjectsAsync(_bucket, listOptions);
            foreach(var albumItem in albumItems.Items.Where(i=>i.IsPrefix))
            {
                albums.Add(new Album() { Name = albumItem.Key });
            }

            return albums;
        }
    }
}
