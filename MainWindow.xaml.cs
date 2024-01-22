using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Opening Window of the application
    /// Copyright: Blaze Devs 2022-2024, All Rights Reserved
    /// </summary>
    /// 
    public static class GlobalVariables
    {
        public static string AppVersion = "v1.6.0"; //Just a variable to store the app version
        public static string AppName = "Filum - DL Application"; //Just a variable to store the app name
        public static bool SpotWindowClicked = false;
    }
    public partial class MainWindow : Window
    {
        
        public MainWindow() //important to have this for the window to work
        {
            var AppName = GlobalVariables.AppName;
            var AppVersion = GlobalVariables.AppVersion;
            this.Title = $"{AppName} {AppVersion} - The ultimate downloader for your needs!";
            this.Loaded += MainWindow_Loaded;
            InitializeComponent();
        }

        readonly HttpClient httpClient = new HttpClient();

        MessageBoxCustom messageBoxCustom = new MessageBoxCustom();

        public string GetTrackIdFromLink(string trackLink)
        {
            // Remove the starting part of the link
            string trimmedLink = trackLink.Replace("https://open.spotify.com/track/", "");

            // Separate the track ID from any additional parameters
            string[] linkParts = trimmedLink.Split('?');
            string trackId = linkParts[0];

            return trackId;
        }
        public string ClientID()
        {
            Secrets secrets = new Secrets();
            string clientID = secrets.SetSpotifyClientID();
            return clientID;
        }
        public string ClientSecret()
        {
            Secrets secrets = new Secrets();
            string clientSecret = secrets.SetSpotifyClientSecret();
            return clientSecret;
        }

        private static readonly string[] Urls = new string[]
        {
            "http://google.com/generate_204",
            "http://bing.com",
            "http://duckduckgo.com",
            "http://yahoo.com",
            "http://github.com"
        };

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            bool isConnected = await IsInternetConnectionActive();
            
            if (isConnected == false)
            {
                MessageBoxCustom.ShowDialogBox("Internet connection is not active! Please check your internet connection and come back to use our application :(", "Internet Connection Check", MessageBoxCustom.MessageBoxType.Error);

                this.Close();
            }
        }
        private async Task<bool> IsInternetConnectionActive()
        {
            //httpClient.Dispose(); //Testing resource for disabling the internet connection
            foreach (var url in Urls)
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Try the next URL
                }
            }

            return false;
        }
        private void YT_Selected(object sender, RoutedEventArgs e)
        {
            exBtn.Visibility = Visibility.Visible;
        }
        private void exBtn_Click(object sender, RoutedEventArgs e)
        {
            if(exType.SelectedItem == YouTube)
            {
                YTWindow yTWindow = new YTWindow();
                yTWindow.Show();
                this.Close();
            }

            else if(exType.SelectedItem == Spotify)
            {
                SpotWindow spotWindow = new SpotWindow();
                spotWindow.Show();
                this.Close();
            }
        }

        private void Spotify_Selected(object sender, RoutedEventArgs e)
        {
            exBtn.Visibility = Visibility.Visible;
        }

        private void DevBtn_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "https://blazedevs.dynu.com/aboutme.html";
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        private void WebBtn_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "https://filumdlwpf.dynu.com";
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        private void Surprise_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "https://youtu.be/dQw4w9WgXcQ";
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }
}
