using StorjPhotoGalleryUploader.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class AlbumListPage : Page
    {
        AlbumListViewModel _viewModel;
        public AlbumListPage()
        {
            this.InitializeComponent();
            DataContext = _viewModel = (AlbumListViewModel)uplink.NET.UnoHelpers.Services.Initializer.GetServiceProvider().GetService(typeof(AlbumListViewModel));
            _viewModel.SetDispatcher(this.Dispatcher);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _viewModel.OnNavigatedAway();
        }
    }
}
