using StorjPhotoGalleryUploader.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using uplink.NET.UnoHelpers.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace StorjPhotoGalleryUploader.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MainPageViewModel _viewModel;

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = _viewModel = (MainPageViewModel)uplink.NET.UnoHelpers.Services.Initializer.GetServiceProvider().GetService(typeof(MainPageViewModel));

            _viewModel.NavigateToAlbumList();
            AppContentFrame.Navigate(typeof(AlbumListPage));
        }

        private void AlbumList_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.NavigateToAlbumList();
            AppContentFrame.Navigate(typeof(AlbumListPage));
        }

        private void CurrentUploadsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.NavigateToCurrentUploads();
            AppContentFrame.Navigate(typeof(CurrentUploadsPage));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.NavigateToSettings();
            AppContentFrame.Navigate(typeof(SettingsPage));
        }
    }
}
