using MvvmGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [ViewModel]
    public partial class MainPageViewModel
    {
        [Property] bool _albumListActive;
        [Property] bool _currentUploadsActive;
        [Property] bool _settingsActive;

        public void NavigateToAlbumList()
        {
            AlbumListActive = true;
            CurrentUploadsActive = false;
            SettingsActive = false;
        }

        public void NavigateToCurrentUploads()
        {
            AlbumListActive = false;
            CurrentUploadsActive = true;
            SettingsActive = false;
        }

        public void NavigateToSettings()
        {
            AlbumListActive = false;
            CurrentUploadsActive = false;
            SettingsActive = true;
        }
    }
}
