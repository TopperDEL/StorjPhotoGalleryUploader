using MvvmGen;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [ViewModel]
    public partial class LoginViewModel
    {
        [Property] private string _accessGrant;
        [Property] private string _bucketName;

        [Command]
        private void Login()
        {
            
        }
    }
}
