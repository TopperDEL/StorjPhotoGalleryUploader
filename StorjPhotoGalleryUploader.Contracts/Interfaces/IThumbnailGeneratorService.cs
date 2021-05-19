using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IThumbnailGeneratorService
    {
        Task<Stream> GenerateThumbnailFromImageAsync(Stream imageStream, int targetWidth);
    }
}
