using MvvmGen;
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

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IAlbumImageViewModelFactory))]
    [Inject(typeof(IAlbumService))]
    [Inject(typeof(IStoreService))]
    [Inject(typeof(IThumbnailGeneratorService))]
    [Inject(typeof(uplink.NET.UnoHelpers.Contracts.Models.AppConfig))]
    [ViewModel]
    public partial class NewAlbumViewModel
    {
        [Property] private string _albumName;
        [Property] private bool _isUploading;
        [Property] private Func<List<Attachment>> _getAttachmentsFunction;

        [Command(CanExecuteMethod = nameof(CanSave))]
        private async Task Save()
        {
            IsUploading = true;

            try
            {
                var attachments = GetAttachmentsFunction();
                var imageNames = attachments.Select(i => i.Filename).ToList();
                var album = await AlbumService.CreateAlbumAsync(AlbumName, imageNames);
                if (album == null)
                {
                    //ToDo: Inform user
                    return;
                }

                bool coverIsUploaded = false;
                foreach (var image in attachments)
                {
                    using (var stream = await image.GetAttachmentStreamAsync())
                    {
                        //Original
                        var uploadedOriginal = await StoreService.PutObjectAsync(AppConfig, album, "pics/original/" + AlbumName + "/" + image.Filename, stream);
                        if (!uploadedOriginal)
                        {
                            //ToDo: Raise error
                            continue;
                        }
                    }

                    using (var stream = await image.GetAttachmentStreamAsync())
                    {
                        //Scaled 1
                        var scaled1 = await ThumbnailGeneratorService.GenerateThumbnailForStreamAsync(stream, "image/jpeg", 1200, 750);
                        var uploadedScaled1 = await StoreService.PutObjectAsync(AppConfig, album, "pics/resized/1200x750/" + AlbumName + "/" + image.Filename, scaled1);
                        if (!uploadedScaled1)
                        {
                            //ToDo: Raise error
                            continue;
                        }

                        //Upload first one as "cover image".
                        //ToDo: let the user select it and make it dynamic so that the image is not uploaded twice.
                        if (!coverIsUploaded)
                        {
                            scaled1.Position = 0;
                            var uploadedScaled2 = await StoreService.PutObjectAsync(AppConfig, album, "pics/resized/1200x750/" + AlbumName + "/cover_image.jpg", scaled1);
                            if (!uploadedScaled2)
                            {
                                //ToDo: Raise error
                                continue;
                            }
                            coverIsUploaded = true;
                        }
                    }

                    using (var stream = await image.GetAttachmentStreamAsync())
                    {
                        //Scaled 2
                        var scaled2 = await ThumbnailGeneratorService.GenerateThumbnailForStreamAsync(stream, "image/jpeg", 360, 225);
                        var uploadedScaled3 = await StoreService.PutObjectAsync(AppConfig, album, "pics/resized/360x225/" + AlbumName + "/" + image.Filename, scaled2);
                        if (!uploadedScaled3)
                        {
                            //ToDo: Raise error
                            continue;
                        }
                    }
                }

                var albumList = await AlbumService.ListAlbumsAsync();
                await AlbumService.RefreshAlbumIndex(albumList);

                await Task.Delay(1000);
                EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
            }
            finally
            {
                IsUploading = false;
            }
        }

        [CommandInvalidate(nameof(AlbumName))]
        private bool CanSave()
        {
            if (IsUploading)
                return false;
            if (GetAttachmentsFunction == null)
                return false;

            var attachments = GetAttachmentsFunction();

            return !string.IsNullOrEmpty(AlbumName) && !AlbumName.Contains("/") && attachments.Count > 0;
        }

        [Command(CanExecuteMethod = nameof(CanCancel))]
        private void Cancel()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
        }

        private bool CanCancel()
        {
            return !IsUploading;
        }
    }
}
