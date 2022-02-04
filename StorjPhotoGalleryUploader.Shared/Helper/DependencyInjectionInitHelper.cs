using Microsoft.Extensions.DependencyInjection;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using StorjPhotoGalleryUploader.Services;
using StorjPhotoGalleryUploader.UWPServices;
using StorjPhotoGalleryUploader.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
            var uploadQueueService = new UploadQueueService(Path.Combine(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path, "uplinkNET.db"));
            services.AddSingleton<IUploadQueueService>(uploadQueueService);

            //Services
            services.AddTransient<IAlbumService, AlbumService>();
            services.AddTransient<IStoreService, StorjStoreService>();
            services.AddTransient<IThumbnailGeneratorService, ThumbnailGeneratorService>();
            services.AddTransient<IPrepareBucketService, PrepareBucketService>();
            services.AddTransient<IAttachmentSelectService, AttachmentSelectService>();
            services.AddTransient<IPhotoUploadService, PhotoUploadService>();
            services.AddSingleton<IShareService, ShareService>();
            services.AddTransient<IOpenBrowserService, OpenBrowserService>();
            services.AddTransient<IDialogService, DialogService>();
            services.AddTransient<ILocalizedTextService, LocalizedTextService>();

            //ViewModels
            services.AddTransient<AlbumListViewModel>();
            services.AddTransient<CreateAlbumViewModel>();
            services.AddTransient<EditAlbumViewModel>();
            services.AddTransient<RenameAlbumViewModel>();
            services.AddTransient<BucketCheckViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<CurrentUploadsViewModel>();
            services.AddTransient<AttachmentContainerViewModel>();
            services.AddTransient<MainPageViewModel>();
            services.AddTransient<SettingsViewModel>();

            //ViewModel-Factories
            services.AddSingleton<IAlbumImageViewModelFactory, AlbumImageViewModelFactory>();
            services.AddSingleton<IAlbumViewModelFactory, AlbumViewModelFactory>();
            services.AddSingleton<IAttachmentViewModelFactory, AttachmentViewModelFactory>();

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
