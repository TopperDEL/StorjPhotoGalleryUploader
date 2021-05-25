using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IThumbnailGeneratorService))]
    [ViewModelGenerateFactory]
    [ViewModel]
    public partial class AlbumImageViewModel
    {
        [Property] private BitmapImage _imageThumbnail = new BitmapImage();
        [Property] private StorageFile _file;
        [Property] private bool _isUploading;
        [Property] private bool _isUploaded;
        [Property] private bool _failedUploading;

        public async Task LoadImageAsync(StorageFile imageFile)
        {
            File = imageFile;
            using (var imageStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var thumb = await ThumbnailGeneratorService.GenerateThumbnailFromImageAsync(imageStream.AsStream(), 500, 500);
                await _imageThumbnail.SetSourceAsync(thumb.AsRandomAccessStream());
            }
        }
    }
}
