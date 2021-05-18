using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IAlbumImageViewModelFactory))]
    [ViewModel]
    public partial class NewAlbumViewModel
    {
        [Property] private string _albumName;
        [Property] private ObservableCollection<AlbumImageViewModel> _albumImages = new ObservableCollection<AlbumImageViewModel>();

        [Command(CanExecuteMethod = nameof(CanSave))]
        private async Task Save()
        {
            //ToDo: save
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
        }

        [CommandInvalidate(nameof(AlbumName))]
        private bool CanSave()
        {
            return !string.IsNullOrEmpty(AlbumName) && !AlbumName.Contains("/");
        }

        [Command]
        private void Cancel()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
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
