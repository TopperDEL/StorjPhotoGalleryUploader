using SkiaSharp;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using StorjPhotoGalleryUploader.Helper;
using System.Threading.Tasks;
using MonkeyCache.FileStore;
using System.Buffers;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace StorjPhotoGalleryUploader.Controls
{
    public sealed partial class SkiaSharpImageControl : UserControl
    {
        private SKBitmap _skiaBmp;

        public string StorjObjectKey
        {
            get { return (string)GetValue(StorjObjectKeyProperty); }
            set { SetValue(StorjObjectKeyProperty, value); }
        }
        public static readonly DependencyProperty StorjObjectKeyProperty =
            DependencyProperty.Register("StorjObjectKey", typeof(string), typeof(SkiaSharpImageControl), new PropertyMetadata("", new PropertyChangedCallback(OnStorjObjectKeyChanged)));

        static SkiaSharpImageControl()
        {
            Barrel.ApplicationId = "STORJ_PHOTO_GALLERY";
        }

        public SkiaSharpImageControl()
        {
            this.InitializeComponent();

            skiaCanvas.PaintSurface += SkiaCanvas_PaintSurface;
        }

        private async void SkiaCanvas_PaintSurface(object sender, SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs e)
        {
            if (_skiaBmp != null)
            {
                e.Surface.Canvas.Clear();
                e.Surface.Canvas.DrawBitmap(_skiaBmp, e.Info.Rect, BitmapStretch.UniformToFill);
            }
        }

        private static void OnStorjObjectKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SkiaSharpImageControl control = d as SkiaSharpImageControl;
            control.OnStorjObjectKeyChanged(e);
        }


        private async void OnStorjObjectKeyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || string.IsNullOrEmpty(e.NewValue.ToString()))
                return;

            await LoadImageAsync(e.NewValue.ToString());
        }

        private async Task LoadImageAsync(string imageKey)
        {
            if (Barrel.Current.IsExpired(imageKey))
            {
                var albumService = (IAlbumService)uplink.NET.UnoHelpers.Services.Initializer.GetServiceProvider().GetService(typeof(IAlbumService));
                var _stream = await albumService.GetImageStreamAsync(imageKey);
                var bytes = ArrayPool<byte>.Shared.Rent((int)_stream.Length);
                await _stream.ReadAsync(bytes, 0, (int)_stream.Length);
                Barrel.Current.Add(imageKey, bytes, TimeSpan.FromDays(180));
                _skiaBmp = SKBitmap.Decode(bytes);
                ArrayPool<byte>.Shared.Return(bytes);
            }
            else
            {
                _skiaBmp = SKBitmap.Decode(Barrel.Current.Get<byte[]>(imageKey));
            }

            skiaCanvas.Invalidate();
        }
    }
}
