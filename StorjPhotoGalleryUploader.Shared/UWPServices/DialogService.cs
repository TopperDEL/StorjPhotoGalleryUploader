using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace StorjPhotoGalleryUploader.UWPServices
{
    public class DialogService : IDialogService
    {
        private static readonly SemaphoreSlim _oneAtATimeAsync = new SemaphoreSlim(1, 1);

        public async Task<bool> AskYesOrNoAsync(string message)
        {
            bool result = false;

            var errorDialog = new ContentDialog
            {
                Title = "Frage",
                Content = message
            }.SetPrimaryButton("Ja", (d, e) => { result = true; }).SetSecondaryButton("Nein", (d, e) => { result = false; });

            await errorDialog.ShowOneAtATimeAsync();

            return result;
        }

        internal static async Task<T> OneAtATimeAsync<T>(Func<Task<T>> show, TimeSpan? timeout, CancellationToken? token)
        {
            var to = timeout ?? TimeSpan.FromHours(1);
            var tk = token ?? new CancellationToken(false);
            if (!await _oneAtATimeAsync.WaitAsync(to, tk))
            {
                throw new Exception($"{nameof(DialogService)}.{nameof(OneAtATimeAsync)} has timed out.");
            }
            try
            {
                return await show();
            }
            finally
            {
                _oneAtATimeAsync.Release();
            }
        }
    }
}
