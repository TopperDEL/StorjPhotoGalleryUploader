using Microsoft.Extensions.DependencyInjection;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using StorjPhotoGalleryUploader.Services;
using StorjPhotoGalleryUploader.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Services;
using uplink.NET.UnoHelpers.Contracts.Interfaces;
using uplink.NET.UnoHelpers.Contracts.Models;
using uplink.NET.UnoHelpers.Services;
using uplink.NET.UnoHelpers.ViewModels;

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

            //Basics
            AddBasics(services);
            services.AddSingleton(appConfig);

            //Uplink-Singletons
            if (!string.IsNullOrEmpty(appConfig.AccessGrant))
            {
                services.AddSingleton(new Access(appConfig.AccessGrant));
            }
            else
            {
                services.AddSingleton(new Access(appConfig.SatelliteAddress, appConfig.ApiKey, appConfig.Secret));
            }
            services.AddSingleton<IBucketService, BucketService>();
            services.AddSingleton<IObjectService, ObjectService>();

            //Services
            services.AddTransient<IAlbumService, AlbumService>();
            services.AddTransient<IStoreService, StorjStoreService>();
            services.AddTransient<IThumbnailGeneratorService, ThumbnailGeneratorService>();
            services.AddTransient<IPrepareBucketService, PrepareBucketService>();

            //ViewModels
            services.AddTransient<AlbumListViewModel>();
            services.AddTransient<NewAlbumViewModel>();
            services.AddTransient<BucketCheckViewModel>();

            //ViewModel-Factories
            services.AddSingleton<IAlbumImageViewModelFactory, AlbumImageViewModelFactory>();

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
            services.AddTransient<LoginViewModel>();

            return services.BuildServiceProvider(true);
        }

        private static void AddBasics(ServiceCollection services)
        {
            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}
