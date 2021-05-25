using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Messages
{
    public class UserLoggedInMessage
    {
        public UserLoggedInMessage(AppConfig appConfig)
        {
            AppConfig = appConfig;
        }

        public AppConfig AppConfig { get; }
    }
}
