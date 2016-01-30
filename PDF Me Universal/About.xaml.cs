using GoogleAnalytics;
using GoogleAnalytics.Core;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PDF_Me_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
            version.Text = "Version: " + String.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major,
                Package.Current.Id.Version.Minor, 
                Package.Current.Id.Version.Build,
                Package.Current.Id.Version.Revision);

        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Tracker myTracker = EasyTracker.GetTracker();
            myTracker.SendView("AboutPage");
            AdDuplex.InterstitialAd interstitialAd = new AdDuplex.InterstitialAd("180815");
            await interstitialAd.LoadAdAsync();
            await interstitialAd.ShowAdAsync();
        }

        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(list.SelectedIndex == 0)
            {
                list.SelectedItem = null;
                Uri u = new Uri("http://www.facebook.com/mtwn1051");
                await Launcher.LaunchUriAsync(u);

            }
            if (list.SelectedIndex == 1)
            {
                list.SelectedItem = null;
                Uri u = new Uri("http://www.twitter.com/mtwn105?s=09");
                await Launcher.LaunchUriAsync(u);

            }
            if (list.SelectedIndex == 2)
            {
                list.SelectedItem = null;
                Uri u = new Uri("http://www.plus.google.com/104579245200931137259");
                await Launcher.LaunchUriAsync(u);

            }
            if (list.SelectedIndex == 3)
            {
                Tracker myTracker = EasyTracker.GetTracker();
                myTracker.SendEvent("Share App", "Spread the word", null, 5);
                list.SelectedItem = null;
                EmailMessage objEmail = new EmailMessage();
               
                objEmail.Body = "Hey, Check out what I have found on the Windows Store \nPDF Me - The Website to PDF Converter \nGo and get it in the store now.";
              
                
                await EmailManager.ShowComposeNewEmailAsync(objEmail);

            }
        }
    }
}
