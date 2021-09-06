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
    [ViewModelGenerateFactory]
    public partial class AlbumViewModel
    {
        [Property] int _imageCount;
        [Property] DateTime _creationDate;
        [Property] string _image1;
        [Property] string _image2;
        [Property] string _image3;
        [Property] string _image4;

        public void SetModel(Album album)
        {
            Model = album;
        }

        public async Task RefreshImageCountAsync()
        {
            var info = await AlbumService.GetAlbumInfoAsync(Model.Name);

            ImageCount = info.ImageCount;
            CreationDate = info.CreationDate;
        }

        public async Task LoadImagesAsync()
        {
            var images = await AlbumService.GetImageKeysAsync(Model.Name, 4, ImageResolution.Small); 
            
            if (images.Count >= 1)
            {
                //First image with higher resolution
                Image1 = "pics/" + ImageResolution.Medium + "/" + Model.Name + "/cover_image.jpg";

                OnPropertyChanged(nameof(Image1));
            }
            if (images.Count >= 2)
            {
                Image2 = images[1];
                OnPropertyChanged(nameof(Image2));
            }
            if (images.Count >= 3)
            {
                Image3 = images[2];
                OnPropertyChanged(nameof(Image3));
            }
            if (images.Count >= 4)
            {
                Image4 = images[3];
                OnPropertyChanged(nameof(Image4));
            }
        }

        [Command]
        public void EditAlbum()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.EditAlbum, Model.Name));
        }
    }
}
