using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Services
{
    public class PrepareBucketService : IPrepareBucketService
    {
        public event PreparationStateChangedEventHandler PreparationStateChangedEvent;

        public async Task<bool> CheckIfBucketNeedsPrepareAsync()
        {
            await Task.Delay(2000);
            return false;
        }

        public async Task<BucketPrepareResult> PrepareBucketAsync()
        {
            await Task.Delay(1000);

            for (int i = 0; i <10; i++)
            {
                PreparationStateChangedEvent?.Invoke(i, 10, "Current step is " + i.ToString());
                await Task.Delay(100);
            }

            return new BucketPrepareResult() { Successfull = false, PrepareErrorMessage ="This is a long and longer info about what might have happened. So show it to the user then..." };
        }
    }
}
