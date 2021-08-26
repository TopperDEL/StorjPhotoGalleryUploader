using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Contracts.Interfaces
{
    public interface IAlbumService
    {
        Task<List<Album>> ListAlbumsAsync();
        Task<Album> CreateAlbumAsync(string albumName, List<string> imageNames);
        Task<bool> RefreshAlbumIndex(List<Album> albums);
        Task<AlbumInfo> GetAlbumInfoAsync(string albumName);
    }
}
