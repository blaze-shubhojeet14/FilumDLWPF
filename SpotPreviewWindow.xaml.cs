using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpotifyAPI.Web;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for SpotPreviewWindow.xaml
    /// </summary>
    public partial class SpotPreviewWindow : Window
    {
        public SpotPreviewWindow()
        {
            InitializeComponent();
        }
        public enum SpotifyTrackType
        {
            Track = 0,
            Album = 1,
            Playlist = 2,
            Episode = 3
        }
        public bool btnClicked;

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

        public static string SanitizeFilename(string input)
        {
            // Remove characters that are not allowed in filenames
            string sanitized = Regex.Replace(input, @"[<>:""/\\|?*]", "");

            // Truncate the filename to a reasonable length (adjust as needed)
            int maxLength = 100;
            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength);
            }

            return sanitized;
        }
         
        public async Task SpotifyTrackPreview(string trackID, SpotifyTrackType spotifyTrackType, string clientID, string clientSecret)
        {
            try
            {
                var config = SpotifyClientConfig.CreateDefault();
                var request = new ClientCredentialsRequest(clientID, clientSecret);
                var response = await new OAuthClient(config).RequestToken(request);
                if (response.IsExpired == true)
                {
                    response = await new OAuthClient(config).RequestToken(request);
                }

                //Spotify client configuration
                var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

                this.Closed += (s, e) => GlobalVariables.SpotWindowClicked = true;

                if (spotifyTrackType == SpotifyTrackType.Track)
                {
                    infoHeader.Content = "Track Information";
                    var track = await spotify.Tracks.Get(trackID);
                    string song = SanitizeFilename(track.Name);
                    string artist = track.Artists[0].Name;
                    string album = track.Album.Name;
                    trackName.Content = "Track Name: " + song;
                    artistName.Content = "Artist Name: " + artist;
                    albumName.Content = "Album Name: " + album;
                    string coverUrl = track.Album.Images[0].Url;
                    coverArt.Source = new BitmapImage(new Uri(coverUrl));
                    await Task.Delay(3000);
                    this.Show();
                }
                else if(spotifyTrackType == SpotifyTrackType.Album)
                {
                    infoHeader.Content = "Album Information";
                    var album = await spotify.Albums.Get(trackID);
                    var albumNameA = SanitizeFilename(album.Name);
                    var artist = album.Artists[0].Name;
                    trackName.Content = "Album Name: " + albumNameA;
                    artistName.Content = "Artist Name: " + artist;
                    albumName.Content = "";
                    string coverUrl = album.Images[0].Url;
                    coverArt.Source = new BitmapImage(new Uri(coverUrl));
                    await Task.Delay(3000);
                    this.Show();

                }
                else if(spotifyTrackType == SpotifyTrackType.Playlist)
                {
                    infoHeader.Content = "Playlist Information";
                    var playlist = await spotify.Playlists.Get(trackID);
                    var playlistName = SanitizeFilename(playlist.Name);
                    var playlistAuthor = playlist.Owner;
                    trackName.Content = "Playlist Name: " + playlistName;
                    artistName.Content = "Author Name: " + playlistAuthor;
                    albumName.Content = "";
                    string coverUrl = playlist.Images[0].Url;
                    coverArt.Source = new BitmapImage(new Uri(coverUrl));
                    await Task.Delay(3000);
                    this.Show();
                }
                else if(spotifyTrackType == SpotifyTrackType.Episode)
                {
                    infoHeader.Content = "Episode Information";
                    var episode = await spotify.Episodes.Get(trackID);
                    var episodeName = SanitizeFilename(episode.Name);
                    var episodeAuthor = episode.Show.Publisher;
                    trackName.Content = "Episode Name: " + episodeName;
                    artistName.Content = "Publisher Name: " + episodeAuthor;
                    albumName.Content = "";
                    string coverUrl = episode.Images[0].Url;
                    coverArt.Source = new BitmapImage(new Uri(coverUrl));
                    await Task.Delay(3000);
                    this.Show();
                }

            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                
            }
           
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            GlobalVariables.SpotWindowClicked = true;
            btnClicked = true;
        }
    }
}
