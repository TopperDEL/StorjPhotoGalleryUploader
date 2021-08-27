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

        private BitmapImage _image1;
        public BitmapImage Image1
        {
            get
            {
                if(_image1 == null)
                {
                    LoadImageAsync(1);
                }
                else
                {
                    return _image1;
                }
                return null;
            }
        }

        private BitmapImage _image2;
        public BitmapImage Image2
        {
            get
            {
                if (_image2 == null)
                {
                    LoadImageAsync(2);
                }
                else
                {
                    return _image2;
                }
                return null;
            }
        }

        private BitmapImage _image3;
        public BitmapImage Image3
        {
            get
            {
                if (_image3 == null)
                {
                    LoadImageAsync(3);
                }
                else
                {
                    return _image3;
                }
                return null;
            }
        }

        private BitmapImage _image4;
        public BitmapImage Image4
        {
            get
            {
                if (_image4 == null)
                {
                    LoadImageAsync(4);
                }
                else
                {
                    return _image4;
                }
                return null;
            }
        }

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

        private async Task LoadImageAsync(int imageNumber)
        {
            if (_images != null)
            {
                var image = new BitmapImage();
#if WINDOWS_UWP
                var stream = (await AlbumService.GetImageStreamAsync(_images[imageNumber-1])).AsRandomAccessStream();
                await image.SetSourceAsync(stream);

#else
                //Todo: This is just a workaround!
                //For any not-yet-found reason the system deadlocks on stream.CopyTo/CopyToAsync
                //even within SetSourceAsync. The deadlock might happen within uplink.NET-library
                var stream = (await AlbumService.GetImageStreamAsync(_images[imageNumber-1]));
                byte[] bytes = new byte[stream.Length];

                stream.Read(bytes, 0, (int)stream.Length);
                MemoryStream mstream = new MemoryStream(bytes);
                await image.SetSourceAsync(mstream);
#endif
                switch(imageNumber)
                {
                    case 1:
                        _image1 = image;
                        OnPropertyChanged(nameof(Image1));
                        break;
                    case 2:
                        _image2 = image;
                        OnPropertyChanged(nameof(Image2));
                        break;
                    case 3:
                        _image3 = image;
                        OnPropertyChanged(nameof(Image3));
                        break;
                    case 4:
                        _image4 = image;
                        OnPropertyChanged(nameof(Image4));
                        break;
                }
            }
        }

        private List<string> _images;
        public async Task LoadImagesAsync()
        {
            _images = await AlbumService.GetImageKeysAsync(Model.Name, 4);
            OnPropertyChanged(nameof(Image1));
            OnPropertyChanged(nameof(Image2));
            OnPropertyChanged(nameof(Image3));
            OnPropertyChanged(nameof(Image4));
        }
    }
}
