using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Models
{
    public class BucketPrepareResult
    {
        public bool Successfull { get; set; }
        public string PrepareErrorMessage { get; set; }
    }
}
