using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using System.Linq;
using Scriban;
using System.Reflection;
using System.IO;
using System.Text;
using uplink.NET.UnoHelpers.Contracts.Models;
using uplink.NET.UnoHelpers.Services;

namespace StorjPhotoGalleryUploader.Services
{
    public class AlbumService : IAlbumService
    {
        readonly IBucketService _bucketService;
        readonly IObjectService _objectService;
        readonly IUploadQueueService _uploadQueueService;
        readonly AppConfig _appConfig;
        private Bucket _bucket;

        public AlbumService(IBucketService bucketService, IObjectService objectService, IUploadQueueService uploadQueueService, AppConfig appConfig)
        {
            _bucketService = bucketService;
            _objectService = objectService;
            _uploadQueueService = uploadQueueService;
            _appConfig = appConfig;
        }

        private async Task InitAsync()
        {
            if (_bucket == null)
            {
                try
                {
                    _bucket = await _bucketService.EnsureBucketAsync(_appConfig.BucketName);
                }
                catch
                {
                    try
                    {
                        _bucket = await _bucketService.GetBucketAsync(_appConfig.BucketName);
                    }
                    catch
                    {

                    }
                }
            }
        }

        public async Task<Album> CreateAlbumAsync(string albumName, List<string> imageNames)
        {
            await InitAsync();

            var accessGrant = _appConfig.TryGetAccessGrant(out bool success);
            if (!success)
                return null;

            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (var indexStream = assembly.GetManifestResourceStream("StorjPhotoGalleryUploader.Services.Assets.site_template.album.index.html"))
            {
                using (StreamReader sr = new StreamReader(indexStream))
                {
                    var albumIndexTemplate = Template.Parse(sr.ReadToEnd());

                    try
                    {
                        var result = await albumIndexTemplate.RenderAsync(new { AlbumName = albumName, ImageNames = imageNames });

                        await _uploadQueueService.AddObjectToUploadQueueAsync(_bucket.Name, albumName + "/index.html", accessGrant, Encoding.UTF8.GetBytes(result), albumName + "/index.html");
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return new Album() { Name = albumName };
        }

        public async Task<List<Album>> ListAlbumsAsync()
        {
            List<Album> albums = new List<Album>();

            await InitAsync();
            if (_bucket == null)
                return albums;

            ListObjectsOptions listOptions = new ListObjectsOptions();
            listOptions.Recursive = false;
            listOptions.Prefix = "pics/original/";
            var albumItems = await _objectService.ListObjectsAsync(_bucket, listOptions);
            foreach (var albumItem in albumItems.Items.Where(i => i.IsPrefix))
            {
                albums.Add(new Album() { Name = albumItem.Key.Replace("pics/original/", "").Replace("/", "") });
            }

            return albums;
        }

        public async Task<bool> RefreshAlbumIndex(List<Album> albums)
        {
            var accessGrant = _appConfig.TryGetAccessGrant(out bool success);
            if (!success)
                return false;

            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (var indexStream = assembly.GetManifestResourceStream("StorjPhotoGalleryUploader.Services.Assets.site_template.homepage.index.html"))
            {
                using (StreamReader sr = new StreamReader(indexStream))
                {
                    var homepageIndexTemplate = Template.Parse(sr.ReadToEnd());

                    try
                    {
                        var result = await homepageIndexTemplate.RenderAsync(new { Albums = albums.Select(a=>new { Name = a.Name, CoverImage = "cover_image.jpg" }).ToList() });

                        await _uploadQueueService.AddObjectToUploadQueueAsync(_bucket.Name, "/index.html", accessGrant, Encoding.UTF8.GetBytes(result), "/index.html");
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
