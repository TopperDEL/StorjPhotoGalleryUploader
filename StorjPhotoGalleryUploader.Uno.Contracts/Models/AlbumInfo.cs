﻿using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Models
{
    public class AlbumInfo
    {
        public int ImageCount { get; set; }
        public DateTime CreationDate { get; set; }
        public string BaseShareUrl { get; set; }
        public string CoverImage { get; set; }
    }
}
