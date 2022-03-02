using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Models;

namespace StorjPhotoGalleryUploader.Helper
{
    public delegate void DownloadStreamCachedEventHandler(CachingDownloadStream sender, byte[] data);
    public class CachingDownloadStream : DownloadStream
    {
        byte[] _cache;
        int _cachedBytes = 0;

        public event DownloadStreamCachedEventHandler DownloadStreamCachedEvent;

        public CachingDownloadStream(Bucket bucket, int totalBytes, string objectName) : base(bucket, totalBytes, objectName)
        {
            _cache = new byte[totalBytes];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readBytes = base.Read(buffer, offset, count);

            if (_cache != null)
            {
                Array.Copy(buffer, 0, _cache, Position - readBytes, readBytes);

                _cachedBytes += readBytes;

                if (_cachedBytes == _cache.Length)
                {
                    DownloadStreamCachedEvent?.Invoke(this, _cache);
                    _cache = null;
                }
            }

            return readBytes;
        }
    }
}
