using GoogleAnalytics;
using GoogleAnalytics.Core;
//using Microsoft.Live;
using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PDF_Me_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class History : Page
    {
        ObservableCollection<Downloads> x  = DatabaseController.getDownloads();
     //   private readonly string[] scopes = new string[] { "onedrive.readwrite", "wl.offline_access", "wl.signin" };
        public History()
        {
            this.InitializeComponent();
            //List<Downloads> downloads = DatabaseController.getDownloads();


            
            downloadList.ItemsSource = x.Reverse(); 
        }

        private void studentList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Downloads download = (Downloads)e.ClickedItem;
            OpenFile(download);
       
         
        }

        private void downloadList_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            FrameworkElement senderelement = sender as FrameworkElement;
            FlyoutBase flyoutbase = FlyoutBase.GetAttachedFlyout(senderelement);
            flyoutbase.ShowAt(senderelement);

        }

        private void downloadList_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderelement = sender as FrameworkElement;
            FlyoutBase flyoutbase = FlyoutBase.GetAttachedFlyout(senderelement);
            flyoutbase.ShowAt(senderelement);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            // var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Downloads download = (Downloads)(e.OriginalSource as FrameworkElement).DataContext;
            //  Type t = datacontext.GetType();
            OpenFile(download);
          
        }
        public async void OpenFile(Downloads download)
        {
            try
            {
               
                StorageFile file = await StorageFile.GetFileFromPathAsync(download.Path);
                await Launcher.LaunchFileAsync(file);
           
            }
      
            catch (Exception ex)
            {
                MessageDialog m = new MessageDialog("The file may be move or deleted.", "File not found");
                Debug.WriteLine("exception" + ex.ToString());
                await m.ShowAsync();
            }
        }

   /*     public async static Task<string> CreateDirectoryAsync(LiveConnectClient client, string folderName, string parentFolder)
        {
            string folderId = null;

            // Retrieves all the directories.
            var queryFolder = parentFolder + "/files?filter=folders,albums";
            var opResult = await client.GetAsync(queryFolder);
            dynamic result = opResult.Result;

            foreach (dynamic folder in result.data)
            {
                // Checks if current folder has the passed name.
                if (folder.name.ToLowerInvariant() == folderName.ToLowerInvariant())
                {
                    folderId = folder.id;
                    break;
                }
            }

            if (folderId == null)
            {
                // Directory hasn't been found, so creates it using the PostAsync method.
                var folderData = new Dictionary<string, object>();
                folderData.Add("name", folderName);
                opResult = await client.PostAsync(parentFolder, folderData);
                result = opResult.Result;

                // Retrieves the id of the created folder.
                folderId = result.id;
            }

            return folderId;
        }
    */ 
    private void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)
        {
            Downloads download = (Downloads)(e.OriginalSource as FrameworkElement).DataContext;
            OpenFolder(download);
        }

        private async void OpenFolder(Downloads download)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(download.Path);
                StorageFolder folder = await file.GetParentAsync();
                await Launcher.LaunchFolderAsync(folder);
            }
            catch (Exception ex)
            {
                MessageDialog m = new MessageDialog("Folder Not Found");
                await m.ShowAsync();
            }
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
           
           
            base.OnNavigatedTo(e);
            Tracker myTracker = EasyTracker.GetTracker();
            myTracker.SendView("HistoryPage");
            if(downloadList.Items.Count == 0)
            {
                nohistoryimage.Visibility = Visibility.Visible;
                nohistory.Visibility = Visibility.Visible;
            }
            else
            {
                nohistoryimage.Visibility = Visibility.Collapsed;
                nohistory.Visibility = Visibility.Collapsed;
            }
            AdDuplex.InterstitialAd interstitialAd = new AdDuplex.InterstitialAd("180815");
            await interstitialAd.LoadAdAsync();
            await interstitialAd.ShowAdAsync();

        }

        private async void MenuFlyoutItem_Click_3(object sender, RoutedEventArgs e)
        {
            Downloads download = (Downloads)(e.OriginalSource as FrameworkElement).DataContext;
            EmailMessage objEmail = new EmailMessage();

            objEmail.Body = "Converted from PDF Me";
             StorageFile fileHandle = await StorageFile.GetFileFromPathAsync(download.Path);
            objEmail.Attachments.Add(new EmailAttachment(fileHandle.Name, fileHandle));
            await EmailManager.ShowComposeNewEmailAsync(objEmail);
        }

        private async void MenuFlyoutItem_Click_4(object sender, RoutedEventArgs e)
        {
            Downloads download = (Downloads)(e.OriginalSource as FrameworkElement).DataContext;
         
            MessageDialog m = new MessageDialog("File Name: \t"+download.FileName+
                "\nDate Created: \t"+download.Date+"\nFile Size: "+"\t"+download.Size+"\nFile Path: \t"+
                download.Path, "File Properties");
            await m.ShowAsync();
        }

        private void C_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }

        private void bannerad_AdRefreshed(object sender, RoutedEventArgs e)
        {
         
        }

        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            /*
            Tracker myTracker = EasyTracker.GetTracker();
            myTracker.SendEvent("OneDrive", "Upload to OneDrive", "Upload to OneDrive Attempted", 1);
            Downloads download = (Downloads)(e.OriginalSource as FrameworkElement).DataContext;
            StorageFile file = await StorageFile.GetFileFromPathAsync(download.Path);
            progress.Visibility = Visibility.Visible;
            pring.IsActive = true;
            uploading.Visibility = Visibility.Visible;
            var authClient = new LiveAuthClient();
            var authResult = await authClient.LoginAsync(new string[] { "wl.skydrive", "wl.skydrive_update" });
            if (authResult.Session == null)
            {
                throw new InvalidOperationException("You need to sign in and give consent to the app.");
            }

            var liveConnectClient = new LiveConnectClient(authResult.Session);
            string skyDriveFolder = await CreateDirectoryAsync(liveConnectClient, "PDF Me - Saved PDFs", "me/skydrive");

            await liveConnectClient.BackgroundUploadAsync(skyDriveFolder, file.Name, file, OverwriteOption.Rename);
            progress.Visibility = Visibility.Collapsed;
            pring.IsActive = false;
            uploading.Visibility = Visibility.Collapsed;
            
            myTracker.SendEvent("OneDrive", "Upload to OneDrive", "Upload to OneDrive Successful", 1);
    */    
    }
    }
    }

