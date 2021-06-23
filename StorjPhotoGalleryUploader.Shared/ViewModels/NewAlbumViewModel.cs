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
        [Property] private ObservableCollection<AlbumImageViewModel> _albumImages = new ObservableCollection<AlbumImageViewModel>();
        [Property] private bool _isUploading;

        [Command(CanExecuteMethod = nameof(CanSave))]
        private async Task Save()
        {
            IsUploading = true;

            try
            {
                var imageNames = AlbumImages.Select(i => i.File.Name).ToList();
                var album = await AlbumService.CreateAlbumAsync(AlbumName, imageNames);
                if(album == null)
                {
                    //ToDo: Inform user
                    return;
                }

                bool coverIsUploaded = false;
                foreach (var image in AlbumImages)
                {
                    image.IsUploading = true;
                    using (var imageStream = await image.File.OpenReadAsync())
                    {
                        //Original
                        var uploadedOriginal = await StoreService.PutObjectAsync(AppConfig, album, "pics/original/" + AlbumName + "/" + image.File.Name, imageStream.AsStream());
                        if (!uploadedOriginal)
                        {
                            //ToDo: Raise error
                            image.FailedUploading = true;
                            image.IsUploading = false;
                            continue;
                        }
                    }

                    using (var imageStream = await image.File.OpenReadAsync())
                    {
                        //Scaled 1
                        var scaled1 = await ThumbnailGeneratorService.GenerateThumbnailFromImageAsync(imageStream.AsStream(), 1200, 750);
                        var uploadedScaled1 = await StoreService.PutObjectAsync(AppConfig, album, "pics/resized/1200x750/" + AlbumName + "/" + image.File.Name, scaled1);
                        if (!uploadedScaled1)
                        {
                            //ToDo: Raise error
                            image.FailedUploading = true;
                            image.IsUploading = false;
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
                                image.FailedUploading = true;
                                image.IsUploading = false;
                                continue;
                            }
                            coverIsUploaded = true;
                        }
                    }

                    using (var imageStream = await image.File.OpenReadAsync())
                    {
                        //Scaled 2
                        var scaled2 = await ThumbnailGeneratorService.GenerateThumbnailFromImageAsync(imageStream.AsStream(), 360, 225);
                        var uploadedScaled2 = await StoreService.PutObjectAsync(AppConfig, album, "pics/resized/360x225/" + AlbumName + "/" + image.File.Name, scaled2);
                        if (!uploadedScaled2)
                        {
                            //ToDo: Raise error
                            image.FailedUploading = true;
                            image.IsUploading = false;
                            continue;
                        }
                    }
                    image.IsUploading = false;
                    image.IsUploaded = true;
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

            return !string.IsNullOrEmpty(AlbumName) && !AlbumName.Contains("/") && AlbumImages.Count > 0;
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

        [Command]
        private async Task AddImagesAsync()
        {
            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            var pickedFiles = await fileOpenPicker.PickMultipleFilesAsync();
            if (pickedFiles.Count > 0)
            {
                // At least one file was picked, you can use them
                foreach (var file in pickedFiles)
                {
                    var albumVM = AlbumImageViewModelFactory.Create();
                    await albumVM.LoadImageAsync(file);
                    AlbumImages.Add(albumVM);
                }
            }
            else
            {
                // No file was picked or the dialog was cancelled.
                return;
            }
        }
    }
}
