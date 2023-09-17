using Microsoft.Win32;
using SpotifyAPI.Web;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using static FilumDLWPF.FolderPicker;

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
        public async void GetMultipleTracks(string clientID, string clientSecret, string ID, SpotifyType spotifyType, string fileFormat)
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
            
            //Get song metadata
            if(spotifyType == SpotifyType.Track)
            {
                var track = await spotify.Tracks.Get(ID);
                string song = SanitizeFilename(track.Name);
                string artist = track.Artists[0].Name;
                string album = track.Album.Name;
                MessageBox.Show($"Track Name : {song} \nArtist : {artist} \nAlbum : {album} ", "Track Information", MessageBoxButton.OK, MessageBoxImage.Information);

                statusBar.Text = "Retrieving Track...";
                //YouTube song search and download
                var result = await youtube.Search.GetVideosAsync(artist + " - " + song);
                string videoID = result.First().Id;

                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoID);
                var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                var streamInfo = new IStreamInfo[] { audioStream };

                var dlg = new FolderPicker();
                dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (dlg.ShowDialog() == true)
                {
                    string filepath = dlg.ResultPath;
                }
             
                string filepathS = $"{dlg.ResultPath}\\{artist} - {song}{fileFormat}";

                statusBar.Text = "Downloading...";
                await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathS).Build());
                statusBar.Text = $"Downloaded  {song} Successfully!";
                MessageBox.Show($"Downloaded {song} successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if(spotifyType == SpotifyType.Album)
            {
                //Get album
                var album = await spotify.Albums.Get(ID);
                var albumName = SanitizeFilename(album.Name);
                var artistA = album.Artists[0].Name;

                statusBar.Text = "Retrieving Album Tracks...";

                MessageBox.Show($"Album Name : {albumName} \nArtist : {artistA} ", "Track Information", MessageBoxButton.OK, MessageBoxImage.Information);


                var dlg = new FolderPicker();
                dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if(dlg.ShowDialog() == true)
                {
                    string filepath = dlg.ResultPath;
                }
                string filepathS = dlg.ResultPath;

                foreach (SimpleTrack item in album.Tracks.Items)
                {
                    var track = await spotify.Tracks.Get(item.Id);
                    string song = SanitizeFilename(track.Name);
                    string artist = track.Artists[0].Name;
                    string albumA = SanitizeFilename(track.Album.Name);
                    string filepathD = $"{filepathS}\\{artist} - {song}{fileFormat}";
                    MessageBox.Show($"Track Name : {song} \nArtist : {artist} \nAlbum : {albumName} ", "Track Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    statusBar.Text = "Retrieving Track...";
                    //YouTube song search and download
                    var result = await youtube.Search.GetVideosAsync(artist + " - " + song);
                    string videoID = result.First().Id;

                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoID);
                    var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfo = new IStreamInfo[] { audioStream };

                    statusBar.Text = "Downloading...";
                    await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathD).Build());
                    statusBar.Text = $"Downloaded {song} from {albumName} Successfully!";
                    MessageBox.Show($"Downloaded {song} from {albumName} successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                statusBar.Text = $"Downloaded the album {albumName} successfully!";
                MessageBox.Show($"Downloaded the album {albumName} Successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
            else if(spotifyType == SpotifyType.Playlist)
            {
                //Get playlist
                var playlist = await spotify.Playlists.Get(ID);
                var playlistName = playlist.Name;
                var playlistAuthor = playlist.Owner;

                statusBar.Text = "Retrieving Playlist Tracks...";

                MessageBox.Show($"Playlist Name: {playlistName} \nPlaylist Owner: {playlistAuthor}", "Track Information", MessageBoxButton.OK, MessageBoxImage.Information);

                var dlg = new FolderPicker();
                dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (dlg.ShowDialog() == true)
                {
                    string filepath = dlg.ResultPath;
                }
                string filepathS = dlg.ResultPath;

                //Get playlist tracks            
                foreach (PlaylistTrack<IPlayableItem> item in playlist.Tracks.Items)
                {

                    if (item.Track is FullTrack track)
                    {
                        var trackA = await spotify.Tracks.Get(track.Id);
                        string song = SanitizeFilename(trackA.Name);
                        string artist = trackA.Artists[0].Name;
                        string albumA = SanitizeFilename(trackA.Album.Name);
                        string filepathD = $"{filepathS}\\{artist} - {song}{fileFormat}";
                        MessageBox.Show($"Track Name : {song} \nArtist : {artist} \nAlbum : {albumA} \nPlaylist : {playlistName} ", "Track Information", MessageBoxButton.OK, MessageBoxImage.Information);

                        statusBar.Text = "Retrieving Track...";
                        //YouTube song search and download
                        var result = await youtube.Search.GetVideosAsync(artist + " - " + song);
                        string videoID = result.First().Id;

                        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoID);
                        var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                        var streamInfo = new IStreamInfo[] { audioStream };

                        statusBar.Text = "Downloading...";
                        await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathD).Build());
                        statusBar.Text = $"Downloaded {song} from {playlistName} Successfully!";
                        MessageBox.Show($"Downloaded {song} from {playlistName} successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    if (item.Track is FullEpisode episode)
                    {
                        // All FullTrack properties are available
                        var trackA = await spotify.Episodes.Get(episode.Id);
                        string episodeName = trackA.Name;
                        string filepathD = $"{filepathS}\\{episodeName}{fileFormat}";
                        MessageBox.Show($"Episode Name : {episodeName} \nPlaylist : {playlistName} ", "Track Information", MessageBoxButton.OK, MessageBoxImage.Information);

                        statusBar.Text = "Retrieving Track...";
                        //YouTube song search and download
                        var result = await youtube.Search.GetVideosAsync(episodeName);
                        string videoID = result.First().Id;

                        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoID);
                        var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                        var streamInfo = new IStreamInfo[] { audioStream };

                        statusBar.Text = "Downloading...";
                        await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathD).Build());
                        statusBar.Text = $"Downloaded {episodeName} from {playlistName} Successfully!";
                        MessageBox.Show($"Downloaded {episodeName} from {playlistName} successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                statusBar.Text = $"Downloaded the playlist {playlistName} successfully!";
                MessageBox.Show($"Downloaded the playlist {playlistName} Successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);

            }
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
            else if (dnType.SelectedItem == Album)
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
                    GetMultipleTracks(ClientID(), ClientSecret(), dlIdSong, SpotifyType.Track, FileFormatSet());
                }
                else if (dnType.SelectedItem == Playlist)
                {
                    GetMultipleTracks(ClientID(), ClientSecret(), dlIdPlaylist, SpotifyType.Playlist, FileFormatSet());
                }
                else if (dnType.SelectedItem == Album)
                {
                    GetMultipleTracks(ClientID(), ClientSecret(), dlIdAlbum, SpotifyType.Album, FileFormatSet());
                }
            }
            catch(Exception ex)
            {
                statusBar.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RMM_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
