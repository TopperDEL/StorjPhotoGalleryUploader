using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [ViewModel]
    public partial class NewAlbumViewModel
    {
        [Property] private string _albumName;

        [Command(CanExecuteMethod = nameof(CanSave))]
        private async Task Save()
        {
            //ToDo: save
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
        }

        [Command]
        private void Cancel()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
        }

        [CommandInvalidate(nameof(AlbumName))]
        private bool CanSave()
        {
            return !string.IsNullOrEmpty(AlbumName) && !AlbumName.Contains("/");
        }
    }
}
