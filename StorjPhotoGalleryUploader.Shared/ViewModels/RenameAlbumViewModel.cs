using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using System.Linq;
using uplink.NET.UnoHelpers.Contracts.Interfaces;
using uplink.NET.UnoHelpers.Contracts.Models;
using uplink.NET.UnoHelpers.Models;
using uplink.NET.UnoHelpers.Messages;
using StorjPhotoGalleryUploader.Contracts.Models;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(IAlbumService))]
    [Inject(typeof(IStoreService))]
    [ViewModel]
    public partial class RenameAlbumViewModel
    {
        [Property] private string _oldAlbumName;
        [Property] private string _newAlbumName;
        [Property] private bool _isSaving;
        [Property] private Func<List<Attachment>> _getAttachmentsFunction;

        [Command(CanExecuteMethod = nameof(CanSave))]
        private async Task Save()
        {
            IsSaving = true;

            try
            {
                await AlbumService.RenameAlbumAsync(OldAlbumName, NewAlbumName);
                
                EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
            }
            finally
            {
                IsSaving = false;
            }
        }

        [CommandInvalidate(nameof(NewAlbumName), nameof(IsSaving))]
        private bool CanSave()
        {
            if (IsSaving)
                return false;

            return !string.IsNullOrEmpty(NewAlbumName) && !NewAlbumName.Contains("/") && OldAlbumName != NewAlbumName && !NewAlbumName.EndsWith(" ");
        }

        [Command(CanExecuteMethod = nameof(CanCancel))]
        private void Cancel()
        {
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
        }

        private bool CanCancel()
        {
            return !IsSaving;
        }
    }
}
