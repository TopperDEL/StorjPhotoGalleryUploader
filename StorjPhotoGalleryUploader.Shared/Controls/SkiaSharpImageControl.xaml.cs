using SkiaSharp;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using StorjPhotoGalleryUploader.Helper;
using System.Threading.Tasks;
using MonkeyCache.FileStore;
using System.Buffers;
using System.Collections.Generic;

namespace StorjPhotoGalleryUploader.Controls
{
    public sealed partial class SkiaSharpImageControl : UserControl
    {
        private static Dictionary<string, SKBitmap> _bitmaps = new Dictionary<string, SKBitmap>();

        //private SKBitmap _skiaBmp;

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

        private void SkiaCanvas_PaintSurface(object sender, SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs e)
        {
            if (StorjObjectKey != null && _bitmaps.ContainsKey(StorjObjectKey))// _skiaBmp != null)
            {
                e.Surface.Canvas.Clear();
                e.Surface.Canvas.DrawBitmap(_bitmaps[StorjObjectKey], e.Info.Rect, BitmapStretch.UniformToFill);
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
            if (!_bitmaps.ContainsKey(imageKey))
            {
                if (Barrel.Current.IsExpired(imageKey))
                {
                    var albumService = (IAlbumService)uplink.NET.UnoHelpers.Services.Initializer.GetServiceProvider().GetService(typeof(IAlbumService));
                    using (var _stream = await albumService.GetImageStreamAsync(imageKey))
                    {
                        var bytes = ArrayPool<byte>.Shared.Rent((int)_stream.Length);
                        await _stream.ReadAsync(bytes, 0, (int)_stream.Length);
                        Barrel.Current.Add(imageKey, bytes, TimeSpan.FromDays(180));
                        var skiaBmp = SKBitmap.Decode(bytes);
                        _bitmaps.Add(imageKey, skiaBmp);
                        ArrayPool<byte>.Shared.Return(bytes);
                    }
                }
                else
                {
                    var skiaBmp = SKBitmap.Decode(Barrel.Current.Get<byte[]>(imageKey));
                    _bitmaps.Add(imageKey, skiaBmp);
                }
            }

            skiaCanvas.Invalidate();
        }
    }
}
