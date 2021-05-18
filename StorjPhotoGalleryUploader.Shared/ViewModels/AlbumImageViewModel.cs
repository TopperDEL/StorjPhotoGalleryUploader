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
    [ViewModelGenerateFactory]
    [ViewModel]
    public partial class AlbumImageViewModel
    {
        [Property] private bool _hasContent;

        [Command]
        private async Task SelectImageAsync()
        {
            HasContent = true;
            EventAggregator.Publish(new ImageAddedMessage());
        }
    }
}
