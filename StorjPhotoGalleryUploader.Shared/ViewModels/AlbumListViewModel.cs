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
    [Inject(typeof(IAlbumViewModelFactory))]
    [ViewModel]
    public partial class AlbumListViewModel
    {
        [Property] private ObservableCollection<AlbumViewModel> _albumList = new ObservableCollection<AlbumViewModel>();
        [Property] private bool _isLoading;
        [Property] private bool _isEmpty;

        [Command]
        private async Task LoadAlbumsAsync()
        {
            IsLoading = true;

            try
            {
                var albums = await AlbumService.ListAlbumsAsync();
                foreach (var album in albums)
                {
                    var vm = AlbumViewModelFactory.Create();
                    vm.SetModel(album);
                    AlbumList.Add(vm);

                    await vm.RefreshImageCountAsync();
                    await vm.LoadImagesAsync();
                }

                if (albums.Count == 0)
                    IsEmpty = true;
                else
                    IsEmpty = false;
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

        [Command]
        private void NavigateCurrentUploads()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.CurrentUploads));
        }
    }
}
