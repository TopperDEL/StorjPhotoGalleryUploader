using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Messages
{
    public enum NavigationTarget
    {
        Login,
        AlbumList,
        NewAlbum,
        CurrentUploads,
        EditAlbum,
        RenameAlbum
    }
    public class DoNavigateMessage
    {
        public DoNavigateMessage(NavigationTarget navigationTarget)
        {
            NavigationTarget = navigationTarget;
        }

        public DoNavigateMessage(NavigationTarget navigationTarget, string parameter)
        {
            NavigationTarget = navigationTarget;
            Parameter = parameter;
        }

        public NavigationTarget NavigationTarget { get; }

        public string Parameter { get; }
    }
}
