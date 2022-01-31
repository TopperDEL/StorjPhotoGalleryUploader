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
        readonly IShareService _shareService;
        private Bucket _bucket;

        public AlbumService(IBucketService bucketService, IObjectService objectService, IUploadQueueService uploadQueueService, AppConfig appConfig, IShareService shareService)
        {
            _bucketService = bucketService;
            _objectService = objectService;
            _uploadQueueService = uploadQueueService;
            _appConfig = appConfig;
            _shareService = shareService;
        }

        private async Task InitAsync()
        {
            if (_bucket == null)
            {
                try
                {
                    _bucket = await _bucketService.EnsureBucketAsync(_appConfig.BucketName).ConfigureAwait(false);
                }
                catch
                {
                    try
                    {
                        _bucket = await _bucketService.GetBucketAsync(_appConfig.BucketName).ConfigureAwait(false);
                    }
                    catch
                    {

                    }
                }
            }
        }

        public async Task<Album> CreateAlbumAsync(string albumName)
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
                        var result = await albumIndexTemplate.RenderAsync(new { AlbumName = albumName }).ConfigureAwait(false);

                        await _uploadQueueService.AddObjectToUploadQueueAsync(_bucket.Name, albumName + "/index.html", accessGrant, Encoding.UTF8.GetBytes(result), albumName + "/index.html").ConfigureAwait(false);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return new Album() { Name = albumName };
        }

        public async Task<Album> RefreshAlbumAsync(string albumName, List<string> imageNames)
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
                        var result = await albumIndexTemplate.RenderAsync(new { AlbumName = albumName, ImageNames = imageNames }).ConfigureAwait(false);

                        //ToDo: Add to customMetadata to UploadQueue
                        //var baseShareUrl = _shareService.CreateAlbumLink(albumName);
                        //var customMetadata = new CustomMetadata();
                        //customMetadata.Entries.Add(new CustomMetadataEntry { Key = "BASE_SHARE_URL", Value = baseShareUrl });

                        await _uploadQueueService.AddObjectToUploadQueueAsync(_bucket.Name, albumName + "/index.html", accessGrant, Encoding.UTF8.GetBytes(result), albumName + "/index.html").ConfigureAwait(false);
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
            listOptions.System = true;
            var albumItems = await _objectService.ListObjectsAsync(_bucket, listOptions).ConfigureAwait(false);
            foreach (var albumItem in albumItems.Items.Where(i => i.IsPrefix))
            {
                albums.Add(new Album()
                {
                    Name = albumItem.Key.Replace("pics/original/", "").Replace("/", "")
                });
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
                        var result = await homepageIndexTemplate.RenderAsync(new { Albums = albums.Select(a => new { Name = a.Name, CoverImage = "cover_image.jpg" }).ToList() });

                        await _uploadQueueService.AddObjectToUploadQueueAsync(_bucket.Name, "index.html", accessGrant, Encoding.UTF8.GetBytes(result), "index.html").ConfigureAwait(false);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task<AlbumInfo> GetAlbumInfoAsync(string albumName)
        {
            await InitAsync();

            ListObjectsOptions listOptions = new ListObjectsOptions();
            listOptions.Recursive = true;
            listOptions.Prefix = "pics/original/" + albumName + "/";
            listOptions.System = true;
            var albumImages = await _objectService.ListObjectsAsync(_bucket, listOptions).ConfigureAwait(false);

            AlbumInfo info = new AlbumInfo();
            info.ImageCount = albumImages.Items.Count;
            info.CreationDate = albumImages.Items.Select(c => c.SystemMetadata).OrderBy(m => m.Created).FirstOrDefault().Created;

            var objectInfo = await _objectService.GetObjectAsync(_bucket, albumName + "/index.html");
            foreach(var entry in objectInfo.CustomMetadata.Entries)
            {
                if(entry.Key == "BASE_SHARE_URL")
                {
                    info.BaseShareUrl = entry.Value;
                }
            }
            if(string.IsNullOrEmpty(info.BaseShareUrl))
            {
                //Create a share-URL and update the Metadata of the album
                info.BaseShareUrl = _shareService.CreateAlbumLink(albumName);
                await RefreshAlbumMetadata(albumName, info.BaseShareUrl);
            }
            return info;
        }

        private async Task RefreshAlbumMetadata(string albumName, string baseShareUrl)
        {
            var index = await _objectService.DownloadObjectAsync(_bucket, albumName + "/index.html", new DownloadOptions());
            var customMetadata = new CustomMetadata();
            customMetadata.Entries.Add(new CustomMetadataEntry { Key = "BASE_SHARE_URL", Value = baseShareUrl });
            var upload = await _objectService.UploadObjectAsync(_bucket, albumName + "/index.html", new UploadOptions(), index.DownloadedBytes, customMetadata,false);
            await upload.StartUploadAsync();
        }

        public async Task<List<string>> GetImageKeysAsync(string albumName, int requestedImageCount, ImageResolution resolution)
        {
            await InitAsync();

            ListObjectsOptions listOptions = new ListObjectsOptions();
            listOptions.Recursive = true;
            listOptions.Prefix = "pics/" + resolution.Value + "/" + albumName + "/";
            listOptions.System = true;
            var albumImages = await _objectService.ListObjectsAsync(_bucket, listOptions).ConfigureAwait(false);

            return albumImages.Items.OrderBy(e=> Guid.NewGuid()).Take(requestedImageCount).Select(i => i.Key).ToList();
        }

        public async Task<Stream> GetImageStreamAsync(string key)
        {
            await InitAsync();

            var objectInfo = await _objectService.GetObjectAsync(_bucket, key).ConfigureAwait(false);

            return new DownloadStream(_bucket, (int)objectInfo.SystemMetadata.ContentLength, key);
        }
    }
}
