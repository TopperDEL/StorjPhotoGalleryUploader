using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.UnoHelpers.Contracts.Interfaces;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(ILoginService))]
    [Inject(typeof(IEventAggregator))]
    [ViewModel]
    public partial class SettingsViewModel
    {

        [Command]
        public void Logout()
        {
            LoginService.Logout();
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.Login));
        }
    }
}
