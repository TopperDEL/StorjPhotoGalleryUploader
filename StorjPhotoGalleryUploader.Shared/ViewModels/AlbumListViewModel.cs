using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IAlbumService))]
    [ViewModel]
    public partial class AlbumListViewModel
    {
        [Property] private ObservableCollection<AlbumViewModel> _albumList = new ObservableCollection<AlbumViewModel>();
        [Property] private bool _isLoading;

        [Command]
        private async Task LoadAlbumsAsync()
        {
            IsLoading = true;

            try
            {
                var albums = await AlbumService.ListAlbumsAsync();
                foreach (var album in albums)
                {
                    AlbumList.Add( new AlbumViewModel(album));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [Command]
        private void NavigateNewAlbum()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.NewAlbum));
        }
    }
}
