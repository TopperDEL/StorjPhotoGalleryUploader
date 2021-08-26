using MvvmGen;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using Windows.UI.Xaml.Media.Imaging;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [ViewModel(typeof(Album))]
    [Inject(typeof(IAlbumService))]
    public partial class AlbumViewModel
    {
        [Property] int _imageCount;
        [Property] DateTime _creationDate;
        [Property] BitmapImage _image1;
        [Property] BitmapImage _image2;
        [Property] BitmapImage _image3;
        [Property] BitmapImage _image4;

        internal AlbumViewModel(Album album, IAlbumService albumService)
        {
            Model = album;
            AlbumService = albumService;
        }

        public async Task RefreshImageCountAsync()
        {
            var info = await AlbumService.GetAlbumInfoAsync(Model.Name);

            ImageCount = info.ImageCount;
            CreationDate = info.CreationDate;
        }

        public async Task LoadImagesAsync()
        {
            var images = await AlbumService.GetImageKeysAsync(Model.Name, 4);

            if (images.Count >= 1)
            {
                var image = new BitmapImage();
                await image.SetSourceAsync((await AlbumService.GetImageStreamAsync(images[0])).AsRandomAccessStream());
                Image1 = image;
            }
            if (images.Count >= 2)
            {
                var image = new BitmapImage();
                await image.SetSourceAsync((await AlbumService.GetImageStreamAsync(images[1])).AsRandomAccessStream());
                Image2 = image;
            }
            if (images.Count >= 3)
            {
                var image = new BitmapImage();
                await image.SetSourceAsync((await AlbumService.GetImageStreamAsync(images[2])).AsRandomAccessStream());
                Image3 = image;
            }
            if (images.Count >= 4)
            {
                var image = new BitmapImage();
                await image.SetSourceAsync((await AlbumService.GetImageStreamAsync(images[3])).AsRandomAccessStream());
                Image4 = image;
            }
        }
    }
}
