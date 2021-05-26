﻿using StorjPhotoGalleryUploader.Contracts.Interfaces;
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
            if (_bucket == null)
            {
                try
                {
                    _bucket = await _bucketService.EnsureBucketAsync(_appConfig.BucketName);
                }
                catch (Exception ex2)
                {
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

        public async Task<Album> CreateAlbumAsync(string albumName, List<string> imageNames)
        {
            await InitAsync();

            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (var indexStream = assembly.GetManifestResourceStream("StorjPhotoGalleryUploader.Services.Assets.site_template.album.index.html"))
            {
                using (StreamReader sr = new StreamReader(indexStream))
                {
                    var albumIndexTemplate = Template.Parse(sr.ReadToEnd());

                    try
                    {
                        var result = await albumIndexTemplate.RenderAsync(new { AlbumName = albumName, ImageNames = imageNames });

                        var upload = await _objectService.UploadObjectAsync(_bucket, albumName + "/index.html", new UploadOptions(), Encoding.UTF8.GetBytes(result), false);
                        await upload.StartUploadAsync();

                    }
                    catch (Exception ex)
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
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (var indexStream = assembly.GetManifestResourceStream("StorjPhotoGalleryUploader.Services.Assets.site_template.homepage.index.html"))
            {
                using (StreamReader sr = new StreamReader(indexStream))
                {
                    var homepageIndexTemplate = Template.Parse(sr.ReadToEnd());

                    try
                    {
                        var result = await homepageIndexTemplate.RenderAsync(new { Albums = albums.Select(a=>new { Name = a.Name, CoverImage = "cover_image.jpg" }).ToList() });

                        var upload = await _objectService.UploadObjectAsync(_bucket, "index.html", new UploadOptions(), Encoding.UTF8.GetBytes(result), false);
                        await upload.StartUploadAsync();

                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}