using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Services
{
    public class ThumbnailGeneratorService : IThumbnailGeneratorService
    {
        public async Task<Stream> GenerateThumbnailFromImageAsync(Stream imageStream, int targetWidth)
        {
            imageStream.Position = 0;
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream))
            {
                image.Mutate(x => x.Resize(targetWidth, 0, KnownResamplers.Lanczos3));
                MemoryStream mstream = new MemoryStream();
                await image.SaveAsync(mstream, new JpegEncoder());

                mstream.Position = 0;
                return mstream;
            }
        }
    }
}
