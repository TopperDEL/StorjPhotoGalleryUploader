using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Messages;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(ILoginService))]
    [Inject(typeof(IEventAggregator))]
    [ViewModel]
    public partial class LoginViewModel
    {
        [Property] private string _accessGrant;
        [Property] private string _bucketName;

        [Command]
        private void Login()
        {
            AppConfig appConfig = new AppConfig(AccessGrant, BucketName);
            var loggedIn = LoginService.Login(appConfig);

            if(loggedIn)
            {
                EventAggregator.Publish(new UserLoggedInMessage(appConfig));
            }
            else
            {
                //ToDo: Raise error
            }
        }
    }
}
