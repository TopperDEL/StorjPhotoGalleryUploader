using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Models
{
    public class AppConfig
    {
        public string BucketName { get; private set; }
        public string AccessGrant { get; private set; }

        public AppConfig(string accessGrant, string bucketName)
        {
            AccessGrant = accessGrant;
            BucketName = bucketName;
        }
    }
}
