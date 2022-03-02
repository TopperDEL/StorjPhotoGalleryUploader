using MvvmGen;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;
using uplink.NET.Models;
using uplink.NET.UnoHelpers.Contracts.Models;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [ViewModel(typeof(Album))]
    [Inject(typeof(IAlbumService))]
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IOpenBrowserService))]
    [Inject(typeof(IDialogService))]
    [Inject(typeof(ILocalizedTextService))]
    [Inject(typeof(IUploadQueueService))]
    [Inject(typeof(IStoreService))]
    [Inject(typeof(AppConfig))]
    [ViewModelGenerateFactory]
    public partial class AlbumViewModel
    {
        [Property] int _imageCount;
        [Property] DateTime _creationDate;
        [Property] string _image1;
        [Property] string _image2;
        [Property] string _image3;
        [Property] string _image4;
        [Property] bool _isInDeletion;
        [Property] bool _hasOnlyOneImage;
        [Property] BitmapImage _image1Bmp;
        [Property] BitmapImage _image2Bmp;
        [Property] BitmapImage _image3Bmp;
        [Property] BitmapImage _image4Bmp;

        private AlbumInfo _albumInfo;

        partial void OnInitialize()
        {
            UploadQueueService.UploadQueueChangedEvent += UploadQueueService_UploadQueueChangedEvent;
        }

        private Windows.UI.Core.CoreDispatcher _dispatcher;
        public void SetDispatcher(Windows.UI.Core.CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        private async void UploadQueueService_UploadQueueChangedEvent(QueueChangeType queueChangeType, UploadQueueEntry entry)
        {
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                switch (queueChangeType)
                {
                    case QueueChangeType.EntryRemoved:
                        if (entry.Identifier.Contains(Model.Name))
                        {
                            await RefreshImageCountAsync();
                            if (ImageCount < 5)
                            {
                                await LoadImagesAsync();
                            }
                        }
                        break;
                }
            });
        }

        public void SetModel(Album album)
        {
            Model = album;
        }

        public async Task RefreshImageCountAsync()
        {
            _albumInfo = await AlbumService.GetAlbumInfoAsync(Model.Name);

            ImageCount = _albumInfo.ImageCount;
            CreationDate = _albumInfo.CreationDate;
        }

        public async Task LoadImagesAsync()
        {
            if (string.IsNullOrEmpty(_albumInfo.BaseShareUrl))
            {
                return;
            }

            var images = await AlbumService.GetImageKeysAsync(Model.Name, 4, ImageResolution.Small, true);

            //URL is storjgallery.de/access/bucket/albumname/#0
            //But it has to be storjgallery.de/access/bucket/
            var baseUrl = _albumInfo.BaseShareUrl.Replace("/#0", "").Replace(Uri.EscapeUriString(Model.Name), "");

            if (images.Count >= 1)
            {
                if (!string.IsNullOrEmpty(_albumInfo.CoverImage))
                {
                    //First image with higher resolution
                    string imageBase;
                    if (_albumInfo.CoverImage.Contains("original"))
                    {
                        imageBase = _albumInfo.CoverImage.Replace("original", "resized/" + ImageResolution.MediumDescription);
                    }
                    else
                    {
                        imageBase = "pics/resized/" + ImageResolution.MediumDescription + "/" + Model.Name + "/" + _albumInfo.CoverImage;
                    }
                    _image1 = Uri.EscapeUriString(baseUrl + imageBase);

                    Image1Bmp = await GenerateBitmapImageFromKeyAsync(imageBase);

                    OnPropertyChanged(nameof(Image1));
                }
                else
                {
                    Image1 = Uri.EscapeUriString(baseUrl + images[0]);
                    Image1Bmp = await GenerateBitmapImageFromKeyAsync(images[0]);
                    OnPropertyChanged(nameof(Image1));
                    images.RemoveAt(0);
                }
            }
            if (images.Count >= 1)
            {
                Image2 = Uri.EscapeUriString(baseUrl + images[0]);
                Image2Bmp = await GenerateBitmapImageFromKeyAsync(images[0]);
                OnPropertyChanged(nameof(Image2));
                images.RemoveAt(0);
            }
            if (images.Count >= 1)
            {
                Image3 = Uri.EscapeUriString(baseUrl + images[0]);
                Image3Bmp = await GenerateBitmapImageFromKeyAsync(images[0]);
                OnPropertyChanged(nameof(Image3));
                images.RemoveAt(0);
            }
            if (images.Count >= 1)
            {
                Image4 = Uri.EscapeUriString(baseUrl + images[0]);
                Image4Bmp = await GenerateBitmapImageFromKeyAsync(images[0]);
                OnPropertyChanged(nameof(Image4));
                images.RemoveAt(0);
            }

            if (ImageCount == 1 && ImageCount != 0)
            {
                HasOnlyOneImage = true;
            }
            else
            {
                HasOnlyOneImage = false;
            }
        }

        private async Task<BitmapImage> GenerateBitmapImageFromKeyAsync(string key)
        {
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
#if WINDOWS_UWP
                await bitmapImage.SetSourceAsync((await StoreService.GetObjectAsStreamAsync(AppConfig, key)).AsRandomAccessStream());
#else
                await bitmapImage.SetSourceAsync(await StoreService.GetObjectAsStreamAsync(AppConfig, key));
#endif
            }
            catch
            {
            }
            return bitmapImage;
        }

        [Command]
        public void EditAlbum()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.EditAlbum, Model.Name));
        }

        [Command]
        public void RenameAlbum()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.RenameAlbum, Model.Name));
        }

        [Command]
        public async Task ViewAlbumInWebAsync()
        {
            if (!string.IsNullOrEmpty(_albumInfo.BaseShareUrl))
            {
                await OpenBrowserService.OpenBrowserAsync(_albumInfo.BaseShareUrl);
            }
        }

        [Command]
        public async Task DeleteAlbumAsync()
        {
            var deleteQuestion = LocalizedTextService.GetLocalizedText("AskForAlbumDeleteQuestion").Replace("&ALBUM_NAME&", Model.Name);
            var title = LocalizedTextService.GetLocalizedText("AskForAlbumDeleteTitle");

            var delete = await DialogService.AskYesOrNoAsync(deleteQuestion, title);
            if (delete)
            {
                IsInDeletion = true;
                await AlbumService.DeleteAlbumAsync(Model.Name);
                EventAggregator.Publish(new AlbumDeletedMessage(Model.Name));
            }
        }

        public void OnNavigatedAway()
        {
            UploadQueueService.UploadQueueChangedEvent -= UploadQueueService_UploadQueueChangedEvent;
        }
    }
}
