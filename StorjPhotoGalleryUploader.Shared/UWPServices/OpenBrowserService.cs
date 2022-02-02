using StorjPhotoGalleryUploader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.UWPServices
{
    public class OpenBrowserService : IOpenBrowserService
    {
        public async Task OpenBrowserAsync(string url)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
        }
    }
}
