using System;
using System.Collections.Generic;
using System.Text;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface ILocalizedTextService
    {
        string GetLocalizedText(string textKey);
    }
}
