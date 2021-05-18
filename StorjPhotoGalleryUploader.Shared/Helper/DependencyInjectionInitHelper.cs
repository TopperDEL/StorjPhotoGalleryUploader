using Microsoft.Extensions.DependencyInjection;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
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
        internal static IServiceProvider ConfigureServices(AppConfig appConfig)
        {
            var services = new ServiceCollection();

            AddBasics(services);
            services.AddSingleton(appConfig);
            services.AddSingleton(new Access(appConfig.AccessGrant));
            services.AddSingleton<IBucketService, BucketService>();
            services.AddSingleton<IObjectService, ObjectService>();
            services.AddTransient<IAlbumService, AlbumService>();
            services.AddTransient<ViewModels.AlbumListViewModel>();
            services.AddTransient<ViewModels.NewAlbumViewModel>();

            return services.BuildServiceProvider(true);
        }

        /// <summary>
        /// Initializes all Services if the user is not yet logged in
        /// </summary>
        /// <returns></returns>
        internal static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            AddBasics(services);
            services.AddTransient<ViewModels.LoginViewModel>();

            return services.BuildServiceProvider(true);
        }

        private static void AddBasics(ServiceCollection services)
        {
            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}
