using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [ViewModelGenerateFactory]
    [ViewModel]
    public partial class AlbumImageViewModel
    {
        [Property] private BitmapImage _imageThumbnail = new BitmapImage();

        public async Task LoadImageAsync(StorageFile imageFile)
        {
            var thumb = await imageFile.GetScaledImageAsThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView, 500);
            await _imageThumbnail.SetSourceAsync(thumb);
        }
    }
}
