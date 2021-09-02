using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IAlbumService
    {
        Task<List<Album>> ListAlbumsAsync();
        Task<Album> CreateAlbumAsync(string albumName);
        Task<Album> RefreshAlbumAsync(string albumName, List<string> imageNames);
        Task<bool> RefreshAlbumIndex(List<Album> albums);
        Task<AlbumInfo> GetAlbumInfoAsync(string albumName);
        Task<List<string>> GetImageKeysAsync(string albumName, int requestedImageCount, ImageResolution resolution);
        Task<Stream> GetImageStreamAsync(string key); 
    }
}
