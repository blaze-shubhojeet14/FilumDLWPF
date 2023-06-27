using Microsoft.Win32;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for SpotWindow.xaml
    /// </summary>
    public partial class SpotWindow : Window
    {
        public SpotWindow()
        {
            InitializeComponent();
        }
        public string SpotifyID { get; set; }
        
        public enum SpotifyType
        {
            Playlist,
            Album,
            Track
        }
        public string FileFormatSet()
        {
            if(ffInfoOpts.SelectedItem == FFmp3)
            {
                return ".mp3";
            }
            else
            {
                return ".mp3";
            }
        }
        
        public string GetPlaylistIdFromLink(string playlistLink)
        {
            // Remove the starting part of the link
            string trimmedLink = playlistLink.Replace("https://open.spotify.com/playlist/", "");

            // Separate the playlist ID from any additional parameters
            string[] linkParts = trimmedLink.Split('?');
            string playlistId = linkParts[0];

            return playlistId;
        }
        public string GetTrackIdFromLink(string trackLink)
        {
            // Remove the starting part of the link
            string trimmedLink = trackLink.Replace("https://open.spotify.com/track/", "");

            // Separate the track ID from any additional parameters
            string[] linkParts = trimmedLink.Split('?');
            string trackId = linkParts[0];

            return trackId;
        }
        public string GetAlbumIdFromLink(string albumLink)
        {
            // Remove the starting part of the link
            string trimmedLink = albumLink.Replace("https://open.spotify.com/album/", "");

            // Separate the album ID from any additional parameters
            string[] linkParts = trimmedLink.Split('?');
            string albumId = linkParts[0];

            return albumId;
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
        public async void GetTrack(string clientID, string clientSecret, string ID, SpotifyType spotifyType, string fileFormat)
        {
            //Authentication
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(clientID, clientSecret);
            var response = await new OAuthClient(config).RequestToken(request);
            if (response.IsExpired == true)
            {
                response = await new OAuthClient(config).RequestToken(request);     
            }
            
            //Spotify client configuration
            var spotify = new SpotifyClient(config.WithToken(response.AccessToken));
            statusBar.Text = "Spotify client has been authenticated successfully";

            //YouTube Client Configuration
            var youtube = new YoutubeClient();
            
            //Get song metada
            var track = await spotify.Tracks.Get(ID);
            string song = track.Name;
            string artist = track.Artists[0].Name;

            statusBar.Text = "Retrieving Track...";
            //YouTube song search and download
            var result = await youtube.Search.GetVideosAsync(artist + " - " + song);
            string videoID = result.First().Id;

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoID);
            var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
            var streamInfo = new IStreamInfo[] { audioStream };

            var saveFileDialog = new SaveFileDialog();
            if (fileFormat == ".mp3")
            {
                saveFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
            }
            else
            {
                saveFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
            }
            
            Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();

            if (filepathDialog == true)
            {
                string filepath = saveFileDialog.FileName;
            }
            string filepathS = saveFileDialog.FileName;

            //Download Logic Depending on SpotifyType

            if(spotifyType == SpotifyType.Track)
            {
                statusBar.Text = "Downloading...";
                await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathS).Build());
                statusBar.Text = "Downloaded " + song +  " Successfully!";
                MessageBox.Show("Downloaded " + song + " successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if(spotifyType == SpotifyType.Playlist)
            {
                statusBar.Text = "Downloading the playlist...";
                await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathS).Build());
                statusBar.Text = "Downloaded " + song + " Successfully!";
                MessageBox.Show("Downloaded " + song + " successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
            
            else if(spotifyType == SpotifyType.Album)
            {
                statusBar.Text = "Downloading the album...";
                await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathS).Build());
                statusBar.Text = "Downloaded " + song + " Successfully!";
                MessageBox.Show("Downloaded " + song +  " Successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            

        }
        public async void GetAlbum(string clientID, string clientSecret, string ID)
        {
            //Authentication
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(clientID, clientSecret);
            var response = await new OAuthClient(config).RequestToken(request);
            if (response.IsExpired == true)
            {
                response = await new OAuthClient(config).RequestToken(request);
            }

            //Spotify client configuration
            var spotify = new SpotifyClient(config.WithToken(response.AccessToken));
            statusBar.Text = "Spotify client has been authenticated successfully";

            //Get album
            var album = await spotify.Albums.Get(ID);
            var albumName = album.Name;

            statusBar.Text = "Retrieving Album Tracks...";
            
            foreach (SimpleTrack item in album.Tracks.Items)
            {
                GetTrack(clientID, clientSecret, item.Id, SpotifyType.Album, FileFormatSet());
            }
            statusBar.Text = "Downloaded the album " + albumName + " successfully!";
            MessageBox.Show("Downloaded the album " + albumName  + " Successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        public async void GetPlaylist(string clientID, string clientSecret, string ID)
        {
            //Authentication
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(clientID, clientSecret);
            var response = await new OAuthClient(config).RequestToken(request);
            if (response.IsExpired == true)
            {
                response = await new OAuthClient(config).RequestToken(request);
            }

            //Spotify client configuration
            var spotify = new SpotifyClient(config.WithToken(response.AccessToken));
            statusBar.Text = "Spotify client has been authenticated successfully";

            //Get playlist
            var playlist = await spotify.Playlists.Get(ID);
            var playlistName = playlist.Name;

            statusBar.Text = "Retrieving Playlist Tracks...";

            //Get playlist tracks            
            foreach (PlaylistTrack<IPlayableItem> item in playlist.Tracks.Items)
            {
                if (item.Track is FullTrack track)
                {
                    // All FullTrack properties are available
                    
                    GetTrack(clientID, clientSecret, track.Id, SpotifyType.Playlist, FileFormatSet());
                }
                if (item.Track is FullEpisode episode)
                {
                    // All FullTrack properties are available
                    GetTrack(clientID, clientSecret, episode.Id, SpotifyType.Playlist, FileFormatSet());
                }
            }
            statusBar.Text = "Downloaded the playlist " + playlistName + " successfully!";
            MessageBox.Show("Downloaded the playlist " + playlistName + " Successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);


        }
        private void ffInfoOpts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dnType.SelectedItem == Song)
            {
                dnButton.Content = "Download";
                dnButton.Visibility = Visibility.Visible;
            }
            else if (dnType.SelectedItem == Playlist)
            {
                dnButton.Content = "Download All";
                dnButton.Visibility = Visibility.Visible;
            }
            else if (dnType.SelectedItem == Album)
            {
                dnButton.Content = "Download All";
                dnButton.Visibility = Visibility.Visible;
            }
        }
        private void dnType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dnType.SelectedItem == Song)
            {
                dnURILabel.Content = "Song URL:";
                placeholder.Content = "Enter the song URL...";
                dnURILabel.Visibility = Visibility.Visible;
                dnURI.Visibility = Visibility.Visible;
                placeholder.Visibility = Visibility.Visible;
                dnURI.Text = "";

            }
            else if (dnType.SelectedItem == Playlist)
            {
                dnURILabel.Content = "Playlist URL:";
                placeholder.Content = "Enter the playlist URL...";
                dnURILabel.Visibility = Visibility.Visible;
                dnURI.Visibility = Visibility.Visible;
                dnURI.Text = "";
                placeholder.Visibility = Visibility.Visible;
            }
            else if (dnType.SelectedItem == Playlist)
            {
                dnURILabel.Content = "Album URL:";
                placeholder.Content = "Enter the album URL...";
                dnURILabel.Visibility = Visibility.Visible;
                dnURI.Visibility = Visibility.Visible;
                dnURI.Text = "";
                placeholder.Visibility = Visibility.Visible;
            }
        }
        private void dnURI_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (dnType.SelectedItem == Song)
            {
                if (dnURI.Text != "")
                {
                    placeholder.Visibility = Visibility.Hidden;
                }
                else
                {
                    placeholder.Visibility = Visibility.Visible;
                }
                string regexPattern = @"^https?:\/\/(?:open\.spotify\.com\/track\/|spotify:track:)([a-zA-Z0-9]+)";
                var regex = new Regex(regexPattern);
                var match = regex.Match(dnURI.Text);
                if (match.Success)
                {
                    string dlIdSong = GetTrackIdFromLink(dnURI.Text);
                    SpotifyID = dlIdSong;
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                }
                else
                {
                    ffInfoLabel.Visibility = Visibility.Hidden;
                    ffInfoOpts.Visibility = Visibility.Hidden;
                }

            }
            else if (dnType.SelectedItem == Playlist)
            {
                if (dnURI.Text != "")
                {
                    placeholder.Visibility = Visibility.Hidden;
                }
                else
                {
                    placeholder.Visibility = Visibility.Visible;
                }
                string regexPattern = @"^https?:\/\/(?:open\.spotify\.com\/playlist\/|spotify:playlist:)([a-zA-Z0-9]+)";
                var regex = new Regex(regexPattern);
                var match = regex.Match(dnURI.Text);
                if (match.Success)
                {
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                }
                else
                {
                    ffInfoLabel.Visibility = Visibility.Hidden;
                    ffInfoOpts.Visibility = Visibility.Hidden;
                }
            }
            else if (dnType.SelectedItem == Album)
            {
                if (dnURI.Text != "")
                {
                    placeholder.Visibility = Visibility.Hidden;
                }
                else
                {
                    placeholder.Visibility = Visibility.Visible;
                }
                string regexPattern = @"^https?:\/\/(?:open\.spotify\.com\/album\/|spotify:album:)([a-zA-Z0-9]+)";
                var regex = new Regex(regexPattern);
                var match = regex.Match(dnURI.Text);
                if (match.Success)
                {
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                }
                else
                {
                    ffInfoLabel.Visibility = Visibility.Hidden;
                    ffInfoOpts.Visibility = Visibility.Hidden;
                }
            }
        }
        private void dnButton_Click(object sender, RoutedEventArgs e)
        {
            string dlIdSong = GetTrackIdFromLink(dnURI.Text);
            string dlIdAlbum = GetAlbumIdFromLink(dnURI.Text);
            string dlIdPlaylist = GetPlaylistIdFromLink(dnURI.Text);
            try
            {
                if (dnType.SelectedItem == Song)
                {
                    GetTrack(ClientID(), ClientSecret(), dlIdSong, SpotifyType.Track, FileFormatSet());
                }
                else if (dnType.SelectedItem == Playlist)
                {
                    GetPlaylist(ClientID(), ClientSecret(), dlIdPlaylist);
                }
                else if (dnType.SelectedItem == Album)
                {
                    GetAlbum(ClientID(), ClientSecret(), dlIdAlbum);
                }
            }
            catch(Exception ex)
            {
                statusBar.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
