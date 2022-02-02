using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Messages
{
    public class AlbumDeletedMessage
    {
        public string AlbumName { get; private set; }

        public AlbumDeletedMessage(string albumName)
        {
            AlbumName = albumName;
        }
    }
}
