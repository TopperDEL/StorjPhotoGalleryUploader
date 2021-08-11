using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.UnoHelpers.Contracts.Interfaces;
using uplink.NET.UnoHelpers.Contracts.Models;
using uplink.NET.UnoHelpers.Models;
using Windows.Storage.Pickers;

namespace StorjPhotoGalleryUploader.Services
{
    public class AttachmentSelectService : IAttachmentSelectService
    {
        public async Task<List<Attachment>> GetAttachmentsAsync()
        {
            List<Attachment> result = new List<Attachment>();

            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            var pickedFiles = await fileOpenPicker.PickMultipleFilesAsync();
            if (pickedFiles.Count > 0)
            {
                // At least one file was picked, you can use them
                foreach (var file in pickedFiles)
                {
                    var attachment = new StorageFileAttachment();
                    attachment.Filename = file.Name;
                    attachment.MimeType = file.ContentType;
                    attachment.StorageFile = file;
                    result.Add(attachment);
                }
            }
            else
            {
                // No file was picked or the dialog was cancelled.
            }

            return result;
        }
    }
}
