using MvvmGen;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.UnoHelpers.Contracts.Interfaces;

namespace StorjPhotoGalleryUploader.ViewModels
{
    [Inject(typeof(ILoginService))]
    [Inject(typeof(IPrepareBucketService))]
    [Inject(typeof(IEventAggregator))]
    [ViewModel]
    public partial class BucketCheckViewModel
    {
        [Property] bool _isChecking;
        [Property] bool _checkSuccessfull;
        [Property] bool _needsUpdate;
        [Property] bool _failed;
        [Property] string _checkError;
        [Property] string _currentStepDescription;
        [Property] int _currentStep;
        [Property] int _totalStepCount;

        [Command]
        private async Task DoChecksAsync()
        {
            bool wasUpdating = false;
            Failed = false;
            IsChecking = true;

            try
            {
                var bucketIsready = await PrepareBucketService.CheckIfBucketIsReadyAsync();
                if (!bucketIsready)
                {
                    wasUpdating = true;
                    CurrentStep = 0;
                    TotalStepCount = 10;

                    NeedsUpdate = true;

                    PrepareBucketService.PreparationStateChangedEvent += PrepareBucketService_PreparationStateChangedEvent;
                    var prepareResult = await PrepareBucketService.PrepareBucketAsync();

                    NeedsUpdate = false;

                    if (!prepareResult.Successfull)
                    {
                        Failed = true;
                        CheckError = prepareResult.PrepareErrorMessage;
                    }
                    else
                    {
                        CheckSuccessfull = true;
                    }
                }
                else
                {
                    CheckSuccessfull = true;
                }
            }
            finally
            {
                IsChecking = false;
                PrepareBucketService.PreparationStateChangedEvent -= PrepareBucketService_PreparationStateChangedEvent;
            }

            if (CheckSuccessfull)
            {
                if (wasUpdating)
                {
                    await Task.Delay(1000);
                }

                EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.AlbumList));
            }
        }

        [Command]
        private void Logout()
        {
            LoginService.Logout();
            EventAggregator.Publish(new DoNavigateMessage(NavigationTarget.Login));
        }

        private void PrepareBucketService_PreparationStateChangedEvent(int currentStep, int totalStepCount, string currentStepDescription)
        {
            CurrentStep = currentStep;
            TotalStepCount = totalStepCount;
            CurrentStepDescription = currentStepDescription;
        }
    }
}
