using MvvmGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [ViewModel]
    public partial class AlbumImageViewModel
    {
        [Property] private bool _hasContent;
    }
}
