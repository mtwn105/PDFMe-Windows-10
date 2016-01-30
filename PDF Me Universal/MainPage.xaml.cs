using GoogleAnalytics;
using GoogleAnalytics.Core;
using Microsoft.OneDrive.Sdk;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
//using Microsoft.Live;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Diagnostics;
using NotificationsExtensions.Toasts;
using Microsoft.QueryStringDotNET;
using Windows.UI.Notifications;
using System.IO;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage.FileProperties;
using Windows.ApplicationModel.Activation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PDF_Me_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Windows.ApplicationModel.DataTransfer.ShareTarget.ShareOperation _shareOperation;
        private readonly string[] _defaultAuthScopes = new string[] { "wl.signin", "wl.skydrive","wl.skydrive_update" };

        public MainPage()
        {
           
            this.InitializeComponent();
            var v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();

            var titleBar = v.TitleBar;
            titleBar.BackgroundColor = Color.FromArgb(255, 211, 47, 47);
            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = Color.FromArgb(255, 211, 47, 47);
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 244, 67, 54);
            hamburger.SelectedIndex = 0;
           

            MyFrame.Navigate(typeof(Page2));
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundColor = Color.FromArgb(255, 211, 47, 47);
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundOpacity = 1;
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            }
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
          //  Object filename = localSettings.Values["filekey"];
            Object theme = localSettings.Values["themekey"];
            Object path = localSettings.Values["pathkey"];
            Object searchp = localSettings.Values["search"];
            Object fn = localSettings.Values["firstname"];
            if (fn != null)
            {
                
            }
          
            if (searchp != null)
            {
                if (searchp.ToString() == "Google")
                {
                    search.SelectedIndex = 0;
                }
                if (searchp.ToString() == "Bing")
                {
                    search.SelectedIndex = 1;
                }
                if (searchp.ToString() == "Wikipedia")
                {
                    search.SelectedIndex = 2;
                }
            }
            /*
            if (filename != null)
            {
                filetext.Text = filename.ToString();
            }
            */
            if (theme != null)
            {
                if (theme.ToString() == "Dark")
                {
                    themetoggle.IsOn = true;
                }
                else
                {
                    themetoggle.IsOn = false;
                }
            }
            if (theme == null)
            {
                if (App.Current.RequestedTheme == ApplicationTheme.Dark)
                {
                    themetoggle.IsOn = true;
                }
                else
                {
                    themetoggle.IsOn = false;
                }
            }
            if (path != null)
            {
                pathtext.Text = path.ToString();
            }
           
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            CheckUserStatus();



            if (e.Parameter != null)
            {
                if (!String.IsNullOrEmpty(e.Parameter.ToString()))
                {
                    // Activate((ShareTargetActivatedEventArgs)e.Parameter);

                    try
                    {

                        Uri absoluteUri = new Uri(e.Parameter.ToString());
                       
                        Tracker myTracker = EasyTracker.GetTracker();
                        myTracker.SendEvent("Downloads", "Share Button Clicked", "Download Attempted", 1);
                        Uri source = new Uri("http://convertmyurl.net/?url=" + absoluteUri);
                        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                        StorageFolder folder;
                        string pathname;
                        object v = localSettings.Values["pathkey"];
                        if (v != null)
                        {
                            pathname = v.ToString();
                            try
                            {
                                folder = await StorageFolder.GetFolderFromPathAsync(pathname);
                            }
                            catch (FileNotFoundException ex)
                            {
                                folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("PDF Me", CreationCollisionOption.OpenIfExists);
                            }
                        }
                        else {
                            folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("PDF Me", CreationCollisionOption.OpenIfExists);

                        }
                        string filename;

                        object value = localSettings.Values["filekey"];
                        if (value != null)
                        {
                            filename = localSettings.Values["filekey"].ToString();
                            Debug.WriteLine("New filename");
                        }
                        else
                        {
                            filename = "PDF Me.pdf";
                            Debug.WriteLine("Default filename");
                        }
                        char[] a = new char[25];
                        StorageFile destinationFile;

                        destinationFile = await folder.CreateFileAsync(
                     "PDF Me.pdf", CreationCollisionOption.GenerateUniqueName);

                        DownloadStart(source, destinationFile);
                        

                    }

                    catch (Exception ex)
                    {

                        Tracker myTracker = EasyTracker.GetTracker();      // Get a reference to tracker.
                        myTracker.SendException(ex.Message, false);
                        MessageDialog m = new MessageDialog(ex.ToString());
                        m.ShowAsync();
                    }

                    //}

                    // }

                }
            }
        }

        private void Activate(ShareTargetActivatedEventArgs e)
        {
            this._shareOperation = e.ShareOperation;
            Window.Current.Content = this;
            Window.Current.Activate();
        }

        public async void DownloadStart(Uri source,StorageFile destinationFile)
        {

            try {
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, destinationFile);
                DownloadStarted();
                await download.StartAsync();

                Tracker myTracker = EasyTracker.GetTracker();
                int progress = (int)(100 * (download.Progress.BytesReceived / (double)download.Progress.TotalBytesToReceive));
                if (progress >= 100)
                {
                    myTracker.SendEvent("Downloads", "Download Finished using Share Target", "Download Successfull using Share Target", 1);

                    BasicProperties basic = await download.ResultFile.GetBasicPropertiesAsync();
                    string size;
                    double siz = basic.Size;
                    //     ulong mb = ulong.Parse(1000000);
                    if (siz > 1000000)
                    {
                        double s = siz / 1000000;
                        size = s.ToString() + "MB";
                    }
                    else
                    {
                        double s = siz / 1000;

                        size = s.ToString() + "KB";
                    }

                    DatabaseController.AddDownload(destinationFile.Name, download.ResultFile.Path, download.ResultFile.DateCreated.DateTime.ToString(), size);

                    DowloadFinish(destinationFile);




                }
                else

                {

                    BasicProperties basic = await download.ResultFile.GetBasicPropertiesAsync();

                    double siz = basic.Size;
                    if (siz == 0)
                    {
                        await destinationFile.DeleteAsync();
                        myTracker.SendEvent("Downloads", "Download Failed due to Server Error", null, 3);
                        MessageDialog m = new MessageDialog("Server is down. Try again later", "Fatal Error");
                        await m.ShowAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                
                Debug.WriteLine(ex.ToString());
               
            }
            /*
            var authClient = new LiveAuthClient();
            var authResult = await authClient.LoginAsync(new string[] { "wl.skydrive", "wl.skydrive_update" });
            if (authResult.Session == null)
            {
                throw new InvalidOperationException("You need to sign in and give consent to the app.");
            }

            var liveConnectClient = new LiveConnectClient(authResult.Session);

            string skyDriveFolder = await CreateDirectoryAsync(liveConnectClient, "PDF Me - Saved PDFs", "me/skydrive");
        */
        }

        private void DowloadFinish(StorageFile destinationFile)
        {
            string title = "Download Successful";
            string content = destinationFile.Name + " is saved in PDF Me folder.";
            ToastVisual visual = new ToastVisual()
            {
                TitleText = new ToastText()
                {
                    Text = title
                },

                BodyTextLine1 = new ToastText()
                {
                    Text = content
                }
            };
            // In a real app, these would be initialized with actual data
            int conversationId = 384928;

            // Construct the actions for the toast (inputs and buttons)
            ToastActionsCustom actions = new ToastActionsCustom()
            {


                Buttons =
    {
        new ToastButton("Open", new QueryString()
        {
            { "action", "open" },
            {"file",destinationFile.Path }

        }.ToString())
        {
            ActivationType = ToastActivationType.Foreground,
           
 
            // Reference the text box's ID in order to
            // place this button next to the text box
            
        },

        new ToastButton("Share", new QueryString()
        {
            { "action", "share" },
              {"file",destinationFile.Path }
        }.ToString())
        {
            ActivationType = ToastActivationType.Foreground
        }


    }
            };
            // Now we can construct the final toast content
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,
                Actions = actions,

                // Arguments when the user taps body of toast
                Launch = new QueryString()
{

{ "conversationId", conversationId.ToString() }

}.ToString()
            };

            // And create the toast notification
            var toast = new ToastNotification(toastContent.GetXml());
            toast.ExpirationTime = DateTime.Now.AddSeconds(10);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async void CheckUserStatus()
        {
            //await AuthenticateUser(true);
        }
        private void DownloadStarted()
        {
            string title = "Download Started";
            int conversationId = 384928;
            string content = " Webpage is being converted to PDF...";
            ToastVisual visual = new ToastVisual()
            {
                TitleText = new ToastText()
                {
                    Text = title
                },

                BodyTextLine1 = new ToastText()
                {
                    Text = content
                }
            };
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,


                // Arguments when the user taps body of toast
                Launch = new QueryString()
{

{ "conversationId", conversationId.ToString() }

}.ToString()
            };
            var toast = new ToastNotification(toastContent.GetXml());
            toast.ExpirationTime = DateTime.Now.AddSeconds(5);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        private void UpdateUI(bool connected, string fullname)
        {
           signintext.Visibility = connected ? Visibility.Collapsed:Visibility.Visible;
            smile.Visibility = connected ? Visibility.Collapsed : Visibility.Visible;
            e.Visibility = connected ? Visibility.Visible : Visibility.Collapsed;
            firstname.Visibility = connected ? Visibility.Visible : Visibility.Collapsed;
            firstname.Text = connected ? fullname : "Not Signed In";
        }
      /*  private async Task LoadProfileImage(LiveConnectClient connectClient)
        {
            try
            {
                LiveDownloadOperation downloadOperation = await connectClient.CreateBackgroundDownloadAsync("me/picture");
                LiveDownloadOperationResult result = await downloadOperation.StartAsync();
                if (result != null && result.Stream != null)
                {
                    using (IRandomAccessStream ras = await result.GetRandomAccessStreamAsync())
                    {
                        BitmapImage imgSource = new BitmapImage();
                        imgSource.SetSource(ras);
                        propic.ImageSource = imgSource;
                    }
                }
            }
            catch (LiveConnectException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        private async Task AuthenticateUser(bool silent)
        {
            signintext.Text = "Signing You In...";
            string text = null;
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object fn = localSettings.Values["firstname"];
            string firstName;
          
                firstName = string.Empty;
          
            bool connected = false;
            try
            {
                var authClient = new LiveAuthClient();
                LiveLoginResult result = silent ? await authClient.InitializeAsync(_defaultAuthScopes) : await authClient.LoginAsync(_defaultAuthScopes);

                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    connected = true;
                    var connectClient = new LiveConnectClient(result.Session);
                    var meResult = await connectClient.GetAsync("me");
                    dynamic meData = meResult.Result;
                    firstName = meData.first_name + " " + meData.last_name;

            

                    localSettings.Values["firstname"] = firstName;
                    await LoadProfileImage(connectClient);
                }
            }
            catch (LiveAuthException ex)
            {
                text = "Error: " + ex.Message;
            }
            catch (LiveConnectException ex)
            {
                text = "Error: " + ex.Message;
            }

            if (text != null)
            {
                var dialog = new Windows.UI.Popups.MessageDialog(text);
                await dialog.ShowAsync();
            }

            UpdateUI(connected, firstName);
        }
        */
        private async void pathbutton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folder = new FolderPicker();
            folder.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folder.FileTypeFilter.Add(".pdf");
            StorageFolder folders = await folder.PickSingleFolderAsync();
            if (folders != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folders);
                pathtext.Text = folders.Path;

            }
        }

       /* private void filetext_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!filetext.Text.Contains(".pdf"))
            {
                filetext.Text += ".pdf";
            }
        }*/
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (splitview.DisplayMode != SplitViewDisplayMode.Inline)
            {
                splitview.IsPaneOpen = !splitview.IsPaneOpen;
            }
            else if (splitview.DisplayMode == SplitViewDisplayMode.Inline)
            {
               
            }
            
        }

        private void hamburger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (hamburger.SelectedIndex == 0)
            {
                pagehead.Text = "Browser";
                MyFrame.Navigate(typeof(Page2));

               
                
            }
            if (hamburger.SelectedIndex == 1)
            {
                pagehead.Text = "History";
                MyFrame.Navigate(typeof(History));
                
            }
            //if (hamburger.SelectedIndex == 2)
            //{
            //    pagehead.Text = "Settings";
            //    MyFrame.Navigate(typeof(Settings));
              
            //}
            if (hamburger.SelectedIndex == 2)
            {
                pagehead.Text = "About";
                MyFrame.Navigate(typeof(About));

            }
            if (hamburger.SelectedIndex == 3)
            {
                pagehead.Text = "Help";
                MyFrame.Navigate(typeof(Help));

            }
            //C#: C Sharp//
            if (splitview.DisplayMode != SplitViewDisplayMode.Inline)
            {
                if (splitview.IsPaneOpen == true)
                {
                    splitview.IsPaneOpen = false;
                }
            }
            else if (splitview.DisplayMode == SplitViewDisplayMode.Inline)
            {
                splitview.IsPaneOpen = true;
            }
        }

        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ratee.SelectedIndex == 0)
            {
                ratee.SelectedItem = null;
                if (signintext.Text == "Sign In")
                {
                    /*
                         ((App)Application.Current).OneDriveClient = OneDriveClientExtensions.GetUniversalClient(this.scopes);
                     if (!((App)Application.Current).OneDriveClient.IsAuthenticated)
                     {
                         await ((App)Application.Current).OneDriveClient.AuthenticateAsync();
                     }
                     Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                         localSettings.Values["signin"] = "yes";
                         signintext.Text = "Sign Out";
                     */
            //        await AuthenticateUser(false);


                }
                else {
                    //TODO
                }/*
                else if(signintext.Text == "Sign Out"){
                    ((App)Application.Current).OneDriveClient = OneDriveClientExtensions.GetUniversalClient(this.scopes);

                    await ((App)Application.Current).OneDriveClient.SignOutAsync();
                    Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                    localSettings.Values["signin"] = "no";
                    signintext.Text = "Sign In";
                }
                */
            }
            if (ratee.SelectedIndex == 1)
            {
                ratee.SelectedItem = null;
            }
                if (ratee.SelectedIndex == 2)
            {
                Tracker myTracker = EasyTracker.GetTracker();
                myTracker.SendEvent("Feedback", "Send Feedback", null, 4);
                ratee.SelectedItem = null;
                EmailMessage objEmail = new EmailMessage();
                string version = "Version: " + String.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major,
               Package.Current.Id.Version.Minor,
               Package.Current.Id.Version.Build,
               Package.Current.Id.Version.Revision);
                objEmail.Body = "Enter Your Feedback here...";
                objEmail.Subject = "PDF Me Universal - Feedback - " + version;
              EmailRecipient em = new EmailRecipient("mtwn105@gmail.com");
                objEmail.To.Add(em);
                await EmailManager.ShowComposeNewEmailAsync(objEmail);

            }
        }

        private void Button_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {

            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (settingview.IsPaneOpen)
            {
              
                settingview.IsPaneOpen=false;

            }
            if (!settingview.IsPaneOpen)
            {

                settingview.IsPaneOpen = true;
            }
        }

        private void settingview_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["themetoggle"] = themetoggle.IsOn.ToString();

        }

        private async void settingview_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //localSettings.Values["filekey"] = filetext.Text;
            if (themetoggle.IsOn)
            {
                localSettings.Values["themekey"] = "Dark";
            }
            else
            {
                localSettings.Values["themekey"] = "Light";

            }
            localSettings.Values["pathkey"] = pathtext.Text;
            if (search.SelectedIndex == 0)
            {
                localSettings.Values["search"] = "Google";
            }
            if (search.SelectedIndex == 1)
            {
                localSettings.Values["search"] = "Bing";
            }
            if (search.SelectedIndex == 2)
            {
                localSettings.Values["search"] = "Wikipedia";
            }
            Object themet = localSettings.Values["themetoggle"];
            if (themet != null)
            {
                if(themet.ToString() != themetoggle.IsOn.ToString())
                {
                    MessageDialog m = new MessageDialog("Restart the app to apply new settings.");
                    m.Commands.Add(new Windows.UI.Popups.UICommand("Close", (command) =>
                    {
                        Application.Current.Exit();

                    }
                            ));
                    await m.ShowAsync();
                }
                else
                {

                }
            }
            else
            {

            }
          
        }
    }
}
