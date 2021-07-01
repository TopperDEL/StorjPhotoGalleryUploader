using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Messages
{
    public enum NavigationTarget
    {
        AlbumList,
        NewAlbum,
        CurrentUploads
    }
    public class DoNavigateMessage
    {
        public DoNavigateMessage(NavigationTarget navigationTarget)
        {
            NavigationTarget = navigationTarget;
        }

        public NavigationTarget NavigationTarget { get; }
    }
}
