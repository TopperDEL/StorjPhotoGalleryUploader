using StorjPhotoGalleryUploader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Models;
using uplink.NET.UnoHelpers.Contracts.Models;
using Windows.ApplicationModel.DataTransfer;

namespace StorjPhotoGalleryUploader.Services
{
    public class ShareService : IShareService
    {
        readonly Access _access;
        readonly AppConfig _appConfig;
        readonly IDialogService _dialogService;
        readonly ILocalizedTextService _localizedTextService;
        private string _shareUrl;
        private string _albumName;
        readonly DataTransferManager _dataTransferManager;

        public ShareService(Access access, AppConfig appConfig, IDialogService dialogService, ILocalizedTextService localizedTextService)
        {
            _access = access;
            _appConfig = appConfig;
            _dialogService = dialogService;
            _localizedTextService = localizedTextService;

            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += DataRequested;
        }

        public string CreateAlbumLink(string albumName)
        {
            var permissions = new Permission();
            permissions.AllowDelete = false;
            permissions.AllowList = false;
            permissions.AllowUpload = false;
            permissions.AllowDownload = true;

            var prefixes = new List<SharePrefix>();
            prefixes.Add(new SharePrefix { Bucket = _appConfig.BucketName, Prefix = "pics/original/" + albumName });
            prefixes.Add(new SharePrefix { Bucket = _appConfig.BucketName, Prefix = "pics/resized/360x225/" + albumName });
            prefixes.Add(new SharePrefix { Bucket = _appConfig.BucketName, Prefix = "pics/resized/1200x750/" + albumName });
            prefixes.Add(new SharePrefix { Bucket = _appConfig.BucketName, Prefix = albumName });
            prefixes.Add(new SharePrefix { Bucket = _appConfig.BucketName, Prefix = "assets/album" });

            var albumAccess = _access.Share(permissions, prefixes);
            try
            {
                var url = albumAccess.CreateShareURL(_appConfig.BucketName, albumName + "/index.html", true, true);

                return url.Replace("gateway", "link").Replace("https://link.storjshare.io/raw","https://storjgallery.de").Replace("index.html","#0");
            }
            catch
            {
                return string.Empty;
            }
        }

        public async void ShowShareUI(string url, string albumName)
        {
            _shareUrl = url;
            _albumName = albumName;

            if (DataTransferManager.IsSupported())
            {
                DataTransferManager.ShowShareUI();
            }
            else
            {
                await _dialogService.ShowErrorMessageAsync(_localizedTextService.GetLocalizedText("CannotShareError"), _localizedTextService.GetLocalizedText("ErrorTitle"));
            }
        }

        private void DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString("ShareTitle").Replace("ALBUMNAME",_albumName);
            args.Request.Data.Properties.Description = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString("ShareDescription");

            args.Request.Data.SetText(Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString("ShareText"));
            args.Request.Data.SetWebLink(new Uri(_shareUrl));
        }
    }
}
