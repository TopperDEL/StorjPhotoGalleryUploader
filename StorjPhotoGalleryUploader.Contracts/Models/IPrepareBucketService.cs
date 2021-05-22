using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StorjPhotoGalleryUploader.Contracts.Models
{
    public delegate void PreparationStateChangedEventHandler(int currentStep, int totalStepCount, string currentStepDescription);
    public interface IPrepareBucketService
    {
        Task<bool> CheckIfBucketIsReadyAsync();
        Task<BucketPrepareResult> PrepareBucketAsync();
        event PreparationStateChangedEventHandler PreparationStateChangedEvent;
    }
}
