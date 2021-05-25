using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IStoreService
    {
        Task<bool> PutObjectAsync(AppConfig appConfig, Album album, string key, Stream objectData);
    }
}
