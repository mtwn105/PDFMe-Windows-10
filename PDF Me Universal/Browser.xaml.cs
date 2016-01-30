using GoogleAnalytics;
using GoogleAnalytics.Core;
//using Microsoft.Live;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using NotificationsExtensions.Toasts;
using Microsoft.QueryStringDotNET;
using Windows.UI.Notifications;
using System.IO;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PDF_Me_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page2 : Page
    {
        public Page2()
        {
            this.InitializeComponent();
         
            browserweb.Navigate(new Uri("http://www.google.com"));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           base.OnNavigatedTo(e);
         
            Tracker myTracker = EasyTracker.GetTracker();
            myTracker.SendView("BrowserPage");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
             Browseweb();
        }

        private async void Browseweb()
        {

            if (urltext.Text == "")
            {

                MessageDialog m = new MessageDialog("Empty Link/URL", "Error");
                await m.ShowAsync();
            }
            else if (urltext.Text.Contains("http://") && urltext.Text.Contains("."))
            {
                browserweb.Navigate(new Uri(urltext.Text));
            }



            else if (!urltext.Text.Contains("http://") && urltext.Text.Contains("."))
            {
                string url = urltext.Text.Insert(0, "http://");
                urltext.Text = url;
                browserweb.Navigate(new Uri(url));
            }
            else if (!urltext.Text.Contains("http://") && !urltext.Text.Contains("."))
            {
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                Object searchp = localSettings.Values["search"];
                if (searchp != null)
                {
                    if (searchp.ToString() == "Google")
                    {
                        browserweb.Navigate(new Uri("http://www.google.com/search?q=" + urltext.Text));
                    }
                    if (searchp.ToString() == "Bing")
                    {
                        browserweb.Navigate(new Uri("http://www.bing.com/search?q=" + urltext.Text));
                    }
                    if (searchp.ToString() == "Wikipedia")
                    {
                        browserweb.Navigate(new Uri("http://en.wikipedia.org/w/index.php?search=" + urltext.Text));
                    }
                }
                else
                {
                    browserweb.Navigate(new Uri("http://www.google.com/search?q=" + urltext.Text));
                }
            }
        }

        private void browserweb_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            nointernet.Visibility = Visibility.Collapsed;
            browserweb.Visibility = Visibility.Visible;
            progress.IsActive = true;
            progress.Visibility = Visibility.Visible;
            urltext.Text = args.Uri.ToString();
            progress.IsActive = true;
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            if (browserweb.CanGoBack)
                browserweb.GoBack();
        }
        //C sharp
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            browserweb.Refresh();
        }

        private void browserweb_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            progress.IsActive = false;
            progress.Visibility = Visibility.Collapsed;
            
        }

        private async void downloadbutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Tracker myTracker = EasyTracker.GetTracker();
              myTracker.SendEvent("Downloads", "Download Button Clicked", "Download Attempted",1);
                Uri source = new Uri("http://convertmyurl.net/?url=" + urltext.Text.Trim());
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                StorageFolder folder;
                string pathname;
                object v = localSettings.Values["pathkey"];
                if (v != null)
                {
                    pathname = v.ToString();
                    try {
                        folder = await StorageFolder.GetFolderFromPathAsync(pathname);
                    }
                    catch(FileNotFoundException ex)
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
                   filename  = localSettings.Values["filekey"].ToString();
                    Debug.WriteLine("New filename");
                }
                else
                {
                    filename = "PDF Me.pdf";
                    Debug.WriteLine("Default filename");
                }
                char[] a = new char[25];
                StorageFile destinationFile;
                if (browserweb.DocumentTitle.Length > 60)
                {
                    destinationFile = await folder.CreateFileAsync(
                 "PDF Me.pdf", CreationCollisionOption.GenerateUniqueName);
                }
                else
                {
                    destinationFile = await folder.CreateFileAsync(
                browserweb.DocumentTitle+".pdf", CreationCollisionOption.GenerateUniqueName);
                }
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, destinationFile);
                downloading.Visibility = Visibility.Visible;
                downloadprogress.Visibility = Visibility.Visible;
                downloadprogress.IsActive = true;
                await download.StartAsync();


                int progress = (int)(100 * (download.Progress.BytesReceived / (double)download.Progress.TotalBytesToReceive));
                if (progress >= 100)
                {
                    myTracker.SendEvent("Downloads", "Download Finished", "Download Successfull", 2);
                    downloading.Visibility = Visibility.Collapsed;
                    downloading.Text = "Downloading: ";
                    downloadprogress.Visibility = Visibility.Collapsed;
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

                    DatabaseController.AddDownload(destinationFile.Name, download.ResultFile.Path,download.ResultFile.DateCreated.DateTime.ToString(),size);
                    AdDuplex.InterstitialAd interstitialAd = new AdDuplex.InterstitialAd("180815");
                    await interstitialAd.LoadAdAsync();
                    /* MessageDialog m = new MessageDialog(destinationFile.Name + " is saved in PDF Me folder.", "Download Completed");
                     m.Commands.Add(new UICommand("Open File", (command) =>
                     {
                          Launcher.LaunchFileAsync(download.ResultFile);
                     }
                     ));

                     m.Commands.Add(new UICommand("Close", (command) =>
                     {

                     }, 0));

                     m.CancelCommandIndex = 0;
                   await  m.ShowAsync();
                   */
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

                    await interstitialAd.LoadAdAsync();
                    await interstitialAd.ShowAdAsync();



                }
                else

                {
                    downloading.Visibility = Visibility.Collapsed;
                    downloadprogress.Visibility = Visibility.Collapsed;
                    BasicProperties basic = await download.ResultFile.GetBasicPropertiesAsync();

                    double siz = basic.Size;
                    if(siz == 0)
                    {
                      await  destinationFile.DeleteAsync();
                        myTracker.SendEvent("Downloads", "Download Failed due to Server Error", null, 3);
                        MessageDialog m = new MessageDialog("Server is down. Try again later","Fatal Error");
                        await   m.ShowAsync();
                    }
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
            catch (Exception ex)
            {
               
                Tracker myTracker = EasyTracker.GetTracker();      // Get a reference to tracker.
                myTracker.SendException(ex.Message, false);
                MessageDialog m = new MessageDialog(ex.ToString());
                m.ShowAsync();
            }
        }

   
        private void urltext_GotFocus(object sender, RoutedEventArgs e)
        {
            urltext.SelectAll();
          
        }

        private void browserweb_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            nointernet.Visibility = Visibility.Visible;
            browserweb.Visibility = Visibility.Collapsed;
        }

        private void urltext_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if(e.Key.ToString() == "Enter")
            {
                Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();
                Browseweb();
            }
        }
    }
}
