using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Models
{
    public class ImageResolution
    {
        public string Value { get; set; }
        private ImageResolution(string value)
        {
            Value = value;
        }

        public static ImageResolution SmallDescription => new ImageResolution("360x225");
        public static ImageResolution MediumDescription => new ImageResolution("1200x750");

        public static ImageResolution Small => new ImageResolution("resized/360x225");
        public static ImageResolution Medium => new ImageResolution("resized/1200x750");
        public static ImageResolution Original => new ImageResolution("original");

        public static implicit operator string(ImageResolution v) => v.Value;
    }
}
