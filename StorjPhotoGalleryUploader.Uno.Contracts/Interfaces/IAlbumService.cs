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
        Task<Album> RefreshAlbumAsync(string albumName, List<string> imageNames, string coverImage = null, bool refreshShareUrl = false);
        Task<bool> RefreshAlbumIndexAsync(List<Album> albums);
        Task<AlbumInfo> GetAlbumInfoAsync(string albumName);
        Task<List<string>> GetImageKeysAsync(string albumName, int requestedImageCount, ImageResolution resolution, bool shuffled);
        Task<Stream> GetImageStreamAsync(string key);
        Task DeleteAlbumAsync(string albumName);
        Task DeleteImageAsync(string albumName, string filename);
        Task SetCoverImageAsync(string albumName, string filename);
        Task RenameAlbumAsync(string oldName, string newName);
    }
}
