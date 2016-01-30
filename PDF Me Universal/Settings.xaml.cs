using GoogleAnalytics;
using GoogleAnalytics.Core;
using System;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PDF_Me_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object filename = localSettings.Values["filekey"];
            Object theme = localSettings.Values["themekey"];
            Object path = localSettings.Values["pathkey"];
            Object searchp = localSettings.Values["search"];
            if(searchp != null)
            {
                if(searchp.ToString()=="Google")
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
            if (filename != null)
            {
                filetext.Text = filename.ToString();
            }
           
            if(theme != null)
            {
                if(theme.ToString() == "Dark")
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
                if(App.Current.RequestedTheme == ApplicationTheme.Dark)
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

        private async void savebutton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            localSettings.Values["filekey"] = filetext.Text;
            if (themetoggle.IsOn)
            {
                localSettings.Values["themekey"] = "Dark";
            }
            else
            {
                localSettings.Values["themekey"] = "Light";

            }
            localSettings.Values["pathkey"] = pathtext.Text;
            localSettings.Values["search"] = search.SelectedItem.ToString();
            MessageDialog m = new MessageDialog("Restart the app to apply new settings.");
            m.Commands.Add(new Windows.UI.Popups.UICommand("Close", (command) =>
            {
                Application.Current.Exit();

            }
                    ));
             await m.ShowAsync();

        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Tracker myTracker = EasyTracker.GetTracker();
            myTracker.SendView("SettingsPage");
            AdDuplex.InterstitialAd interstitialAd = new AdDuplex.InterstitialAd("180815");
            await interstitialAd.LoadAdAsync();
            await interstitialAd.ShowAdAsync();
        }
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

        private void filetext_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!filetext.Text.Contains(".pdf"))
            {
                filetext.Text += ".pdf";
            }
        }
    }
}
