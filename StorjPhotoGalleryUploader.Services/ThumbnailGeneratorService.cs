using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
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
        public async Task<Stream> GenerateThumbnailFromImageAsync(Stream imageStream, int targetWidth, int targetHeight)
        {
            imageStream.Position = 0;
            using (SKBitmap sourceBitmap = SKBitmap.Decode(imageStream))
            {
                int height = Math.Min(targetHeight, sourceBitmap.Height);
                int width = Math.Min(targetWidth, sourceBitmap.Width);

                using (SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
                {
                    using (SKImage scaledImage = SKImage.FromBitmap(scaledBitmap))
                    {
                        using (SKData data = scaledImage.Encode())
                        {
                            return new MemoryStream(data.ToArray());
                        }
                    }
                }
            }
        }
    }
}
