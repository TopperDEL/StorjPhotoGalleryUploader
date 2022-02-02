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
    public partial class AlbumListViewModel: IEventSubscriber<AlbumDeletedMessage>
    {
        [Property] private ObservableCollection<AlbumViewModel> _albumList = new ObservableCollection<AlbumViewModel>();
        [Property] private bool _isLoading;
        [Property] private bool _isEmpty;

        private Windows.UI.Core.CoreDispatcher _dispatcher;
        public void SetDispatcher(Windows.UI.Core.CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

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
                    vm.SetDispatcher(_dispatcher);
                    AlbumList.Add(vm);

                    await vm.RefreshImageCountAsync();
                    await vm.LoadImagesAsync();
                }

                if (albums.Count == 0)
                    IsEmpty = true;
                else
                    IsEmpty = false;
            }
            catch (Exception ex)
            {

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

        public async void OnEvent(AlbumDeletedMessage eventData)
        {
            foreach(var albumVM in AlbumList)
            {
                if(albumVM.Name == eventData.AlbumName)
                {
                    AlbumList.Remove(albumVM);
                    albumVM.OnNavigatedAway();
                    return;
                }
            }
        }

        public void OnNavigatedAway()
        {
            foreach (var albumVM in AlbumList)
            {
                albumVM.OnNavigatedAway();
            }
        }
    }
}
