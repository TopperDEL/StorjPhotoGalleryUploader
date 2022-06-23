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
using uplink.NET.UnoHelpers.Messages;
using StorjPhotoGalleryUploader.Contracts.Models;
using uplink.NET.UnoHelpers.ViewModels;
using System.Buffers;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IShareService))]
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IAlbumImageViewModelFactory))]
    [Inject(typeof(IAlbumService))]
    [Inject(typeof(IStoreService))]
    [Inject(typeof(IThumbnailGeneratorService))]
    [Inject(typeof(IPhotoUploadService))]
    [Inject(typeof(AppConfig))]
    [Inject(typeof(IAttachmentViewModelFactory))]
    [Inject(typeof(IDialogService))]
    [Inject(typeof(ILocalizedTextService))]
    [ViewModel]
    public partial class EditAlbumViewModel : IEventSubscriber<AttachmentAddedMessage>, IEventSubscriber<AttachmentAddingFinishedMessage>, IEventSubscriber<AttachmentDeletedMessage>, IEventSubscriber<AttachmentSetAsCoverMessage>
    {
        [Property] private string _albumName;
        [Property] private bool _hasImages;
        [Property] private bool _isUploading;
        [Property] private Func<List<Attachment>> _getAttachmentsFunction;
        [Property] private Action _selectImagesAction;
        [Property] private Action<AttachmentViewModel> _addAttachmentAction;

        partial void OnInitialize()
        {
            LoadExistingAttachmentsAsync();
        }

        private bool isInitializing = true;
        private async Task LoadExistingAttachmentsAsync()
        {
            while (AlbumName == null)
            {
                await Task.Delay(100);
            }
            var albumInfo = await AlbumService.GetAlbumInfoAsync(AlbumName);

            if (string.IsNullOrEmpty(albumInfo.BaseShareUrl))
            {
                return;
            }

            //URL is storjgallery.de/access/bucket/albumname/#0
            //But it has to be storjgallery.de/access/bucket/
            var baseUrl = albumInfo.BaseShareUrl.Replace("/#0", "").Replace(Uri.EscapeUriString(AlbumName), "");

            var images = await AlbumService.GetImageKeysAsync(AlbumName, int.MaxValue, ImageResolution.Small, false);
            foreach (var image in images)
            {
                var attachment = new Attachment();
                var attachmentVm = AttachmentViewModelFactory.Create();
                attachment.Filename = Uri.EscapeUriString(image);
                attachment.MimeType = "image/jpeg";
                attachment.AttachmentData = await StoreService.GetObjectAsStreamAsync(AppConfig, attachment.Filename);
                await attachmentVm.SetAttachmentWithThumbnailAsync(attachment, attachment.AttachmentData);

                AddAttachmentAction(attachmentVm);
                HasImages = true;
            }

            isInitializing = false;
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

        public async void OnEvent(AttachmentAddedMessage eventData)
        {
            if (isInitializing)
                return;

            IsUploading = true;
            try
            {
                var attachments = GetAttachmentsFunction().ToList();
                if (attachments.Count() == 0)
                    return;
                var attachment = attachments.Last();
                if (attachment == null)
                    return;

                //First: Upload the smallest one
                using (var stream = await attachment.GetAttachmentStreamAsync())
                {
                    if (stream != null)
                    {
                        await PhotoUploadService.CreateAndUploadAsync(AlbumName, attachment.Filename, stream, ImageResolution.Small);
                    }
                }
                
                //Then upload the medium one
                using (var stream = await attachment.GetAttachmentStreamAsync())
                {
                    if (stream != null)
                    {
                        await PhotoUploadService.CreateAndUploadAsync(AlbumName, attachment.Filename, stream, ImageResolution.Medium);
                    }
                }

                //Then the original one
                using (var stream = await attachment.GetAttachmentStreamAsync())
                {
                    if (stream != null)
                    {
                        await PhotoUploadService.CreateAndUploadAsync(AlbumName, attachment.Filename, stream, ImageResolution.Original);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                IsUploading = false;
            }
        }

        public async void OnEvent(AttachmentAddingFinishedMessage eventData)
        {
            //Refresh the album html
            await RefreshAlbumAsync();
        }

        public async Task RefreshAlbumAsync()
        {
            var attachments = GetAttachmentsFunction();
            var imageNames = attachments.Select(i => i.Filename.Replace("pics/resized/360x225/" + AlbumName + "/", "")).ToList();
            string coverImage = null;
            if (!HasImages)
            {
                if(imageNames.Count > 0)
                {
                    coverImage = imageNames.First();
                }
                HasImages = true;
            }
            var album = await AlbumService.RefreshAlbumAsync(AlbumName, coverImage);

            var albumList = await AlbumService.ListAlbumsAsync();
            await AlbumService.RefreshAlbumIndexAsync(albumList);
        }

        [Command]
        public void SelectImages()
        {
            if (SelectImagesAction != null)
            {
                SelectImagesAction.Invoke();
            }
        }

        [Command]
        private async Task ShareAlbum()
        {
            string url;

            var albumInfo = await AlbumService.GetAlbumInfoAsync(AlbumName);
            if (!string.IsNullOrEmpty(albumInfo.BaseShareUrl))
            {
                url = albumInfo.BaseShareUrl;
            }
            else
            {
                url = ShareService.CreateAlbumLink(AlbumName);
            }
            if (!string.IsNullOrEmpty(url))
            {
                ShareService.ShowShareUI(url, AlbumName);
            }
            else
            {
                await DialogService.ShowErrorMessageAsync(LocalizedTextService.GetLocalizedText("CouldNotCreateShareURLError"), LocalizedTextService.GetLocalizedText("ErrorTitle"));
            }
        }

        public async void OnEvent(AttachmentDeletedMessage eventData)
        {
            await AlbumService.DeleteImageAsync(AlbumName, eventData.DeletedAttachment.Filename);
        }

        public async void OnEvent(AttachmentSetAsCoverMessage eventData)
        {
            await AlbumService.SetCoverImageAsync(AlbumName, eventData.SelectedAttachment.Filename);
        }
    }
}
