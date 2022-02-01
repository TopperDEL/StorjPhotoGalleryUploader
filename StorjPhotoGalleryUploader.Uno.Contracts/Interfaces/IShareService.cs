using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IShareService
    {
        string CreateAlbumLink(string albumName);
        void ShowShareUI(string url, string albumName);
    }
}
