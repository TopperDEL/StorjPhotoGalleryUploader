using MvvmGen;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [ViewModel(typeof(Album))]
    public partial class AlbumViewModel
    {
        [Property] int _imageCount;

        internal AlbumViewModel(Album album)
        {
            Model = album;
        }
    }
}
