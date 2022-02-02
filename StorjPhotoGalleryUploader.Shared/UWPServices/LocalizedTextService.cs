using StorjPhotoGalleryUploader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.UWPServices
{
    public class LocalizedTextService : ILocalizedTextService
    {
        public string GetLocalizedText(string textKey)
        {
            return Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString(textKey);
        }
    }
}
