﻿using Microsoft.Extensions.DependencyInjection;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Services;
using StorjPhotoGalleryUploader.UnoAppServices;
using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Services;

namespace StorjPhotoGalleryUploader.Helper
{
    static class DependencyInjectionInitHelper
    {
        /// <summary>
        /// Initializes all Services once the user is logged in.
        /// </summary>
        /// <param name="access">The Storj DCS-Access object to use</param>
        /// <returns></returns>
        internal static IServiceProvider ConfigureServices(Access access)
        {
            var services = new ServiceCollection();

            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton(access);
            services.AddSingleton<IBucketService, BucketService>();
            services.AddSingleton<IObjectService, ObjectService>();
            services.AddTransient<IAlbumService, AlbumService>();
            services.AddTransient<ViewModels.AlbumListViewModel>();

            return services.BuildServiceProvider(true);
        }

        /// <summary>
        /// Initializes all Services if the user is not yet logged in
        /// </summary>
        /// <returns></returns>
        internal static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ILoginService, LoginService>();

            return services.BuildServiceProvider(true);
        }
    }
}
