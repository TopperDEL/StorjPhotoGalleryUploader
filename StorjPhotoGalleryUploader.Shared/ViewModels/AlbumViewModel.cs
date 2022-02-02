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
using System.Linq;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [ViewModel(typeof(Album))]
    [Inject(typeof(IAlbumService))]
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IOpenBrowserService))]
    [ViewModelGenerateFactory]
    public partial class AlbumViewModel
    {
        [Property] int _imageCount;
        [Property] DateTime _creationDate;
        [Property] string _image1;
        [Property] string _image2;
        [Property] string _image3;
        [Property] string _image4;

        private AlbumInfo _albumInfo;

        public void SetModel(Album album)
        {
            Model = album;
        }

        public async Task RefreshImageCountAsync()
        {
            _albumInfo = await AlbumService.GetAlbumInfoAsync(Model.Name);

            ImageCount = _albumInfo.ImageCount;
            CreationDate = _albumInfo.CreationDate;
        }

        public async Task LoadImagesAsync()
        {
            if(string.IsNullOrEmpty(_albumInfo.BaseShareUrl))
            {
                return;
            }

            var images = await AlbumService.GetImageKeysAsync(Model.Name, 4, ImageResolution.Small, true);

            //URL is storjgallery.de/access/bucket/albumname/#0
            //But it has to be storjgallery.de/access/bucket/
            var baseUrl = _albumInfo.BaseShareUrl.Replace("/#0", "").Replace(Uri.EscapeUriString(Model.Name), "");

            if (images.Count >= 1)
            {
                //First image with higher resolution
                Image1 = baseUrl + "pics/" + ImageResolution.Medium + "/" + Model.Name + "/cover_image.jpg";

                OnPropertyChanged(nameof(Image1));
            }
            if (images.Count >= 2)
            {
                Image2 = baseUrl + images[1];
                OnPropertyChanged(nameof(Image2));
            }
            if (images.Count >= 3)
            {
                Image3 = baseUrl + images[2];
                OnPropertyChanged(nameof(Image3));
            }
            if (images.Count >= 4)
            {
                Image4 = baseUrl + images[3];
                OnPropertyChanged(nameof(Image4));
            }
        }

        [Command]
        public void EditAlbum()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.EditAlbum, Model.Name));
        }

        [Command]
        public async Task ViewAlbumInWebAsync()
        {
            if (!string.IsNullOrEmpty(_albumInfo.BaseShareUrl))
            {
                await OpenBrowserService.OpenBrowserAsync(_albumInfo.BaseShareUrl);
            }
        }
    }
}
