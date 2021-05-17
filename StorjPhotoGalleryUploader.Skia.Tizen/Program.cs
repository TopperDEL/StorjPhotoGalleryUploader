using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace StorjPhotoGalleryUploader.Skia.Tizen
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new StorjPhotoGalleryUploader.App(), args);
            host.Run();
        }
    }
}
