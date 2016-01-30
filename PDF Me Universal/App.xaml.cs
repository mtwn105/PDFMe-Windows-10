using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Diagnostics;
using Microsoft.OneDrive.Sdk;
using Microsoft.QueryStringDotNET;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Generic;
using GoogleAnalytics.Core;
using GoogleAnalytics;
using System.IO;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage.FileProperties;
using NotificationsExtensions.Toasts;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace PDF_Me_Universal
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private List<IStorageItem> storageItems = new List<IStorageItem>();
        public IOneDriveClient OneDriveClient { get; set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            InitializeComponent();
            Suspending += OnSuspending;
            try
            {
                DatabaseController.CreateTable();
                Debug.WriteLine("App.xaml.cs Excecuted");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            // value = "Default";
            Object value = localSettings.Values["themekey"];
            CheckPath();
            
            if (value != null)
            {
                if (value.ToString() == "Dark")
                {
                    Application.Current.RequestedTheme = ApplicationTheme.Dark;
                    Debug.WriteLine("Dark theme");
                }
                else if (value.ToString() == "Light")
                {
                    Application.Current.RequestedTheme = ApplicationTheme.Light;
                    Debug.WriteLine("Light theme");
                }
                else
                {
                    Application.Current.RequestedTheme = ApplicationTheme.Light;
                    Debug.WriteLine("Default theme");
                }
            }
        }

        private async void CheckPath()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object path = localSettings.Values["pathkey"];
            if (path == null)
            {
                StorageFolder folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("PDF Me", CreationCollisionOption.OpenIfExists);
                localSettings.Values["pathkey"] = folder.Path.ToString();
            }
        }





        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;
            AdDuplex.AdDuplexClient.Initialize("a9a21c15-728b-4063-ae7d-f8eacd5c927b");
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
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        
           
        }

      
        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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
        protected async override void OnActivated(IActivatedEventArgs e)
        {
            // Get the root frame
            Frame rootFrame = Window.Current.Content as Frame;

            // TODO: Initialize root frame just like in OnLaunched

            // Handle toast activation
            if (e is ToastNotificationActivatedEventArgs)
            {
                var toastActivationArgs = e as ToastNotificationActivatedEventArgs;

                // Parse the query string
                QueryString args = QueryString.Parse(toastActivationArgs.Argument);

                // See what action is being requested 
                switch (args["action"])
                {
                    // Open the file
                    case "open":

                        // The URL retrieved from the toast args
                        string path = args["file"];

                        StorageFile downloadedfile = await StorageFile.GetFileFromPathAsync(path);
                        await Launcher.LaunchFileAsync(downloadedfile);
                        break;


                    // Open the conversation
                    case "share":
                        string path1 = args["file"];
                        // The conversation ID retrieved from the toast args
                        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                        dataTransferManager.DataRequested += DataTransferManager_DataRequested;


                        StorageFile downloaddfile = await StorageFile.GetFileFromPathAsync(path1);
                       
                        storageItems.Add(downloaddfile);
                        DataTransferManager.ShowShareUI();
                        break;
                }

            }

            // TODO: Handle other types of activation

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = "Sharing PDF from PDF Me";
            //   request.Data.Properties.Description = "Demonstrates how to share files.";

            // Because we are making async calls in the DataRequested event handler,
            // we need to get the deferral first.
            DataRequestDeferral deferral = request.GetDeferral();

            // Make sure we always call Complete on the deferral.
            try
            {
                request.Data.SetStorageItems(storageItems);
            }
            finally
            {

                deferral.Complete();
            }
            }

        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
          
            string absoluteUri = "http://www.google.com";
            var shareOperation = args.ShareOperation;
            if (shareOperation.Data.Contains(StandardDataFormats.Text))
            {
                var uri = await shareOperation.Data.GetTextAsync();
                if (uri != null)
                {
                    absoluteUri = uri;
                }
            }
            if (shareOperation.Data.Contains(StandardDataFormats.Uri))
            {
                var uri = await shareOperation.Data.GetUriAsync();
                if (uri != null)
                {
                    absoluteUri = uri.AbsoluteUri;
                }
            }
            //Notify shareOperation that we are completed as soon as possible
           // shareOperation.ReportCompleted();
            var rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage),absoluteUri.ToString());
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
           
        }

    }
    
}
