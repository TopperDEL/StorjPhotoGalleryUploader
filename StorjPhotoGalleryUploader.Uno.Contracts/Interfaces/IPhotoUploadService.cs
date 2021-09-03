using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IPhotoUploadService
    {
        Task CreateAndUploadAsync(string albumName, string fileName, Stream imageStream, ImageResolution resolution);
        Task CreateAndUploadCoverImageAsync(string albumName, string fileName, Stream imageStream, ImageResolution resolution);
    }
}
