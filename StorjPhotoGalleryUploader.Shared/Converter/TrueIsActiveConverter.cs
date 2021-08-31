using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace StorjPhotoGalleryUploader.Converter
{
    public class TrueIsActiveConverter : IValueConverter
    {
        public SolidColorBrush ActiveColor { get; set; }
        public SolidColorBrush InactiveColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value == true)
                return ActiveColor;
            else
                return InactiveColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
