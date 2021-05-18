using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IAlbumImageViewModelFactory))]
    [ViewModel]
    public partial class NewAlbumViewModel: IEventSubscriber<ImageAddedMessage>
    {
        [Property] private string _albumName;
        [Property] private ObservableCollection<AlbumImageViewModel> _albumImages = new ObservableCollection<AlbumImageViewModel>();

        [Command]
        private async Task InitAlbum()
        {
            AlbumImages.Add(AlbumImageViewModelFactory.Create());
        }

        [Command(CanExecuteMethod = nameof(CanSave))]
        private async Task Save()
        {
            //ToDo: save
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
        }

        [CommandInvalidate(nameof(AlbumName))]
        private bool CanSave()
        {
            return !string.IsNullOrEmpty(AlbumName) && !AlbumName.Contains("/");
        }

        [Command]
        private void Cancel()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
        }

        public void OnEvent(ImageAddedMessage eventData)
        {
            AlbumImages.Add(AlbumImageViewModelFactory.Create());
        }
    }
}
