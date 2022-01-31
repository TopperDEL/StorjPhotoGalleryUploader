using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmGen.Events;
using StorjPhotoGalleryUploader.Contracts.Messages;
using StorjPhotoGalleryUploader.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using uplink.NET.Interfaces;
using uplink.NET.UnoHelpers.Contracts.Interfaces;
using uplink.NET.UnoHelpers.Messages;
using uplink.NET.UnoHelpers.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace StorjPhotoGalleryUploader
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application, IEventSubscriber<UserLoggedInMessage>, IEventSubscriber<DoNavigateMessage>, IEventSubscriber<ErrorOccuredMessage>, IEventSubscriber<NavigateBackFromCurrentUploadsMessage>
    {
        private const string STORJPHOTOGALLERY_RESOURCE = "STORJPHOTOGALLERY";

#if NET5_0 && WINDOWS
        private Window _window;

#else
        private Windows.UI.Xaml.Window _window;
#endif

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeLogging();
            this.UnhandledException += App_UnhandledException;

            try
            {
                this.InitializeComponent();
            }
            catch(Exception ex)
            {

            }

#if __IOS__
            //Initialize the uplink.NET-library
            uplink.NET.Models.Access.Init_iOs(Foundation.NSBundle.MainBundle.BundlePath);
#endif

#if HAS_UNO || NETFX_CORE
            this.Suspending += OnSuspending;
#endif
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] = Windows.UI.Colors.White;

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

#if NET5_0 && WINDOWS
            _window = new Window();
            _window.Activate();
#else
            _window = Windows.UI.Xaml.Window.Current;
#endif

            var rootFrame = _window.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                _window.Content = rootFrame;
            }

#if !(NET5_0 && WINDOWS)
            if (e.PrelaunchActivated == false)
#endif
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    var services = Helper.DependencyInjectionInitHelper.ConfigureServices();
                    uplink.NET.UnoHelpers.Services.Initializer.Init(services, STORJPHOTOGALLERY_RESOURCE);

                    var eventAggregator = uplink.NET.UnoHelpers.Services.Initializer.GetServiceProvider().GetService<IEventAggregator>();
                    eventAggregator.RegisterSubscriber(this);

                    var loginService = uplink.NET.UnoHelpers.Services.Initializer.GetServiceProvider().GetService<ILoginService>();
                    if (loginService.GetIsLoggedIn())
                    {
                        var appConfig = loginService.GetLogin();
                        OnEvent(new UserLoggedInMessage(appConfig));
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(LoginPage), e.Arguments);
                    }
                }
                // Ensure the current window is active
                _window.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
                builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
                builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
                builder.AddDebug();
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // RemoteControl and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            //global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
        }

        public void OnEvent(UserLoggedInMessage loggedInMessage)
        {
            var services = Helper.DependencyInjectionInitHelper.ConfigureServices(loggedInMessage.AppConfig);
            uplink.NET.UnoHelpers.Services.Initializer.Init(services, STORJPHOTOGALLERY_RESOURCE);
            var eventAggregator = uplink.NET.UnoHelpers.Services.Initializer.GetServiceProvider().GetService<IEventAggregator>();
            eventAggregator.RegisterSubscriber(this);

            var uploadQueueService = uplink.NET.UnoHelpers.Services.Initializer.GetServiceProvider().GetService<IUploadQueueService>();
            uploadQueueService.ProcessQueueInBackground();

            DoNavigate(typeof(BucketCheckPage));
        }

        public void OnEvent(DoNavigateMessage navigationData)
        {
            Type pageType = null;
            switch(navigationData.NavigationTarget)
            {
                case NavigationTarget.Login:
                    pageType = typeof(LoginPage);
                    break;
                case NavigationTarget.NewAlbum:
                    pageType = typeof(CreateAlbumPage);
                    break;
                case NavigationTarget.EditAlbum:
                    pageType = typeof(EditAlbumPage);
                    break;
                case NavigationTarget.AlbumList:
                    pageType = typeof(MainPage);
                    break;
                case NavigationTarget.CurrentUploads:
                    pageType = typeof(CurrentUploadsPage);
                    break;
            }

            if (pageType != null)
                DoNavigate(pageType, navigationData.Parameter);
        }

        private void DoNavigate(Type pageType, string parameter = null)
        {
            var rootFrame = _window.Content as Frame;

            rootFrame.Navigate(pageType, parameter);
        }

        public async void OnEvent(ErrorOccuredMessage eventData)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = eventData.ErrorMessage
            };

            errorDialog.PrimaryButtonText = "Ok";
            errorDialog.PrimaryButtonStyle = this.Resources["ButtonRoundedStyle"] as Style;

            await errorDialog.ShowAsync();
        }

        public void OnEvent(NavigateBackFromCurrentUploadsMessage eventData)
        {
            OnEvent(new DoNavigateMessage(NavigationTarget.AlbumList));
        }
    }
}
