using Microsoft.Extensions.DependencyInjection;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Services;
using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Services;

namespace StorjPhotoGalleryUploader.Helper
{
    static class DependencyInjectionInitHelper
    {
        internal static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IBucketService, BucketService>();
            services.AddSingleton<IObjectService, ObjectService>();
            services.AddTransient<IAlbumService, AlbumService>();
            services.AddTransient<ViewModels.AlbumListViewModel>();

            return services.BuildServiceProvider(true);
        }
    }
}
