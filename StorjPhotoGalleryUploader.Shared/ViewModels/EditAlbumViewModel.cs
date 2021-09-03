﻿using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using System.Linq;
using uplink.NET.UnoHelpers.Contracts.Interfaces;
using uplink.NET.UnoHelpers.Contracts.Models;
using uplink.NET.UnoHelpers.Models;
using uplink.NET.UnoHelpers.Messages;
using StorjPhotoGalleryUploader.Contracts.Models;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IAlbumImageViewModelFactory))]
    [Inject(typeof(IAlbumService))]
    [Inject(typeof(IStoreService))]
    [Inject(typeof(IThumbnailGeneratorService))]
    [Inject(typeof(IPhotoUploadService))] //einbinden

    //    wenn erstes foto: mache cover draus
    //lade sofort das kleinste hoch
    //im hintergrund die zwei anderen größen

    //beim laden alle kleinen bilder laden
    //menü "als cover setzen"
    [Inject(typeof(uplink.NET.UnoHelpers.Contracts.Models.AppConfig))]
    [ViewModel]
    public partial class EditAlbumViewModel : IEventSubscriber<AttachmentAddedMessage>
    {
        [Property] private string _albumName;
        [Property] private bool _hasImages;
        [Property] private bool _isUploading;
        [Property] private Func<List<Attachment>> _getAttachmentsFunction;
        [Property] private Action _selectImagesAction;


        [Command(CanExecuteMethod = nameof(CanCancel))]
        private void Cancel()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
        }

        private bool CanCancel()
        {
            return !IsUploading;
        }

        public async void OnEvent(AttachmentAddedMessage eventData)
        {
            IsUploading = true;

            try
            {
                var attachments = GetAttachmentsFunction().ToList();
                var attachment = attachments.Last();

                //First: Upload the smallest one
                using (var stream = await attachment.GetAttachmentStreamAsync())
                {
                    await PhotoUploadService.CreateAndUploadAsync(AlbumName, attachment.Filename, stream, ImageResolution.Small);
                }

                //Then: Upload the cover, if this is the first image
                if (!HasImages)
                {
                    using (var stream = await attachment.GetAttachmentStreamAsync())
                    {
                        await PhotoUploadService.CreateAndUploadCoverImageAsync(AlbumName, attachment.Filename, stream, ImageResolution.Small);
                    }
                }

                //At this point we have at least one image
                HasImages = true;

                //Then upload the medium one
                using (var stream = await attachment.GetAttachmentStreamAsync())
                {
                    await PhotoUploadService.CreateAndUploadAsync(AlbumName, attachment.Filename, stream, ImageResolution.Medium);
                }

                //Then the original one
                using (var stream = await attachment.GetAttachmentStreamAsync())
                {
                    await PhotoUploadService.CreateAndUploadAsync(AlbumName, attachment.Filename, stream, ImageResolution.Original);
                }

                //After that, refresh the album html
                await RefreshAlbumAsync();
            }
            finally
            {
                IsUploading = false;
            }
        }

        public async Task RefreshAlbumAsync()
        {
            var attachments = GetAttachmentsFunction();
            var imageNames = attachments.Select(i => i.Filename).ToList();
            var album = await AlbumService.RefreshAlbumAsync(AlbumName, imageNames);

            var albumList = await AlbumService.ListAlbumsAsync();
            await AlbumService.RefreshAlbumIndex(albumList);
        }

        [Command]
        public void SelectImages()
        {
            if (SelectImagesAction != null)
            {
                SelectImagesAction.Invoke();
            }
        }
    }
}
