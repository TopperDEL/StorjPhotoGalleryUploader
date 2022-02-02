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
        private const string BASE_SHARE_URL = "BASE_SHARE_URL";
        private const string COVER_IMAGE = "COVER_IMAGE";
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
            return await RefreshAlbumAsync(albumName, new List<string>()); //Simply no images, yet
        }

        public async Task<Album> RefreshAlbumAsync(string albumName, List<string> imageNames, string coverImage = null)
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

                        string baseShareUrl = string.Empty;

                        //If the object already exists, get its BaseShareUrl
                        try
                        {
                            var existingIndexObject = await _objectService.GetObjectAsync(_bucket, albumName + "/index.html");
                            if (existingIndexObject != null)
                            {
                                baseShareUrl = existingIndexObject.CustomMetadata.Entries.Where(e => e.Key == BASE_SHARE_URL).First().Value;
                            }
                        }
                        catch
                        { }
                        if (string.IsNullOrEmpty(baseShareUrl))
                        {
                            //Create the share-URL, that is used to share the album and
                            //to load the thumbnails for the app.
                            baseShareUrl = _shareService.CreateAlbumLink(albumName);
                        }

                        var customMetadata = new CustomMetadata();
                        customMetadata.Entries.Add(new CustomMetadataEntry { Key = BASE_SHARE_URL, Value = baseShareUrl });
                        if(coverImage != null)
                        {
                            customMetadata.Entries.Add(new CustomMetadataEntry { Key = COVER_IMAGE, Value = coverImage });
                        }

                        var upload = await _objectService.UploadObjectAsync(_bucket, albumName + "/index.html", new UploadOptions(), Encoding.UTF8.GetBytes(result), customMetadata, false);
                        await upload.StartUploadAsync();
                        //await _uploadQueueService.AddObjectToUploadQueueAsync(_bucket.Name, albumName + "/index.html", accessGrant, Encoding.UTF8.GetBytes(result), albumName + "/index.html", customMetadata).ConfigureAwait(false);
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
            listOptions.Recursive = true;
            listOptions.System = true;
            var albumItems = await _objectService.ListObjectsAsync(_bucket, listOptions).ConfigureAwait(false);
            foreach (var albumItem in albumItems.Items.Where(i => i.Key.Contains("/index.html")).OrderByDescending(i=>i.SystemMetadata.Created))
            {
                albums.Add(new Album()
                {
                    Name = albumItem.Key.Replace("/index.html", "")
                });
            }

            return albums;
        }

        public async Task<bool> RefreshAlbumIndexAsync(List<Album> albums)
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
            if (albumImages.Items.Count > 0)
            {
                info.CreationDate = albumImages.Items.Select(c => c.SystemMetadata).OrderBy(m => m.Created).FirstOrDefault().Created;
            }
            else
            {
                info.CreationDate = DateTime.Now;
            }

            try
            {
                var objectInfo = await _objectService.GetObjectAsync(_bucket, albumName + "/index.html");
                foreach (var entry in objectInfo.CustomMetadata.Entries)
                {
                    if (entry.Key == BASE_SHARE_URL)
                    {
                        info.BaseShareUrl = entry.Value;
                    }
                    else if(entry.Key == COVER_IMAGE)
                    {
                        info.CoverImage = entry.Value;
                    }
                }
                if (string.IsNullOrEmpty(info.BaseShareUrl))
                {
                    //Create a share-URL
                    info.BaseShareUrl = _shareService.CreateAlbumLink(albumName);
                }
                if(string.IsNullOrEmpty(info.CoverImage) && albumImages.Items.Count > 0)
                {
                    info.CoverImage = albumImages.Items[0].Key;
                }
            }
            catch
            {
                //Ignore errors - album would simply not yet show images correctly. Should update on next
                //jump to album list
            }
            return info;
        }

        public async Task<List<string>> GetImageKeysAsync(string albumName, int requestedImageCount, ImageResolution resolution, bool shuffled)
        {
            await InitAsync();

            ListObjectsOptions listOptions = new ListObjectsOptions();
            listOptions.Recursive = true;
            listOptions.Prefix = "pics/" + resolution.Value + "/" + albumName + "/";
            listOptions.System = true;
            var albumImages = await _objectService.ListObjectsAsync(_bucket, listOptions).ConfigureAwait(false);

            if (shuffled)
            {
                return albumImages.Items.OrderBy(e => Guid.NewGuid()).Take(requestedImageCount).Select(i => i.Key).ToList();
            }
            else
            {
                return albumImages.Items.Take(requestedImageCount).Select(i => i.Key).ToList();
            }
        }

        public async Task<Stream> GetImageStreamAsync(string key)
        {
            await InitAsync();

            var objectInfo = await _objectService.GetObjectAsync(_bucket, key).ConfigureAwait(false);

            return new DownloadStream(_bucket, (int)objectInfo.SystemMetadata.ContentLength, key);
        }

        public async Task DeleteImageAsync(string albumName, string filename)
        {
            await DeleteImageAsync(albumName, filename.Replace("resized/"+ImageResolution.SmallDescription,"original"), ImageResolution.Original);
            await DeleteImageAsync(albumName, filename.Replace(ImageResolution.SmallDescription, ImageResolution.MediumDescription), ImageResolution.Medium);
            await DeleteImageAsync(albumName, filename, ImageResolution.Small);

            var images = await GetImageKeysAsync(albumName, int.MaxValue, ImageResolution.Small, false);
            var coverImage = (await GetAlbumInfoAsync(albumName)).CoverImage;
            await RefreshAlbumAsync(albumName, images, coverImage);
        }

        private async Task DeleteImageAsync(string albumName, string filename, ImageResolution resolution)
        {
            var keys = await GetImageKeysAsync(albumName, int.MaxValue, resolution, false);
            foreach (var key in keys.Where(k=>k.Contains(filename)))
            {
                await _objectService.DeleteObjectAsync(_bucket, key).ConfigureAwait(false);
            }
        }

        public async Task DeleteAlbumAsync(string albumName)
        {
            await DeleteImagesAsync(albumName, ImageResolution.Original);
            await DeleteImagesAsync(albumName, ImageResolution.Medium);
            await DeleteImagesAsync(albumName, ImageResolution.Small);
            await _objectService.DeleteObjectAsync(_bucket, albumName + "/index.html");
            var albumList = await ListAlbumsAsync();
            await RefreshAlbumIndexAsync(albumList);
        }

        private async Task DeleteImagesAsync(string albumName, ImageResolution resolution)
        {
            var keys = await GetImageKeysAsync(albumName, int.MaxValue, resolution, false);
            foreach (var key in keys)
            {
                await _objectService.DeleteObjectAsync(_bucket, key).ConfigureAwait(false);
            }
        }

        public async Task SetCoverImageAsync(string albumName, string filename)
        {
            var images = await GetImageKeysAsync(albumName, int.MaxValue, ImageResolution.Small, false);
            await RefreshAlbumAsync(albumName, images, filename);
        }
    }
}
