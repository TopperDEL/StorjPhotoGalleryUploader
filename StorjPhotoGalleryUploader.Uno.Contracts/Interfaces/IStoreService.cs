﻿using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.UnoHelpers.Contracts.Models;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IStoreService
    {
        Task<bool> PutObjectAsync(AppConfig appConfig, string key, Stream objectData, string identifier);
        Task<Stream> GetObjectAsStreamAsync(AppConfig appConfig, string key);
    }
}
