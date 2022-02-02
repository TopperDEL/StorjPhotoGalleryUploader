using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IDialogService
    {
        Task<bool> AskYesOrNoAsync(string message);
    }
}
