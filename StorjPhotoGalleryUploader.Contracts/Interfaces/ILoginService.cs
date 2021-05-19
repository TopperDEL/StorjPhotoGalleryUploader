using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface ILoginService
    {
        bool GetIsLoggedIn();
        AppConfig GetLogin();
        bool Login(AppConfig appConfig);
        void Logout();
    }
}
