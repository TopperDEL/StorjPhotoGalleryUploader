using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IOpenBrowserService
    {
        Task OpenBrowserAsync(string url);
    }
}
