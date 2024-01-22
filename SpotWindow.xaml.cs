using Microsoft.Win32;
using SpotifyAPI.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TagLib;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using static FilumDLWPF.FolderPicker;
using static FilumDLWPF.SpotPreviewWindow;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for SpotWindow.xaml
    /// Window for the Spotify Downloader
    /// Copyright: Blaze Devs 2022-2024, All Rights Reserved
    /// </summary>
    /// 

    public partial class SpotWindow : Window
    {
        public SpotWindow()
        {
            var AppName = GlobalVariables.AppName;
            var AppVersion = GlobalVariables.AppVersion;
            this.Title = $" {AppName} {AppVersion} - Spotify Downloader";
            InitializeComponent();

        }
        public string SpotifyID { get; set; }
        SpotPreviewWindow previewWindow = new SpotPreviewWindow();

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
        public void MergeThumbnailWithSong(string songPath, string thumbnailPath, string title, string artist, string album)
        {
            var file = TagLib.File.Create(songPath);
            var pictures = new IPicture[]
            {
            new Picture(thumbnailPath)
           {
            Description = "Cover",
            Type = PictureType.FrontCover,
            MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg
           }};

            if (file.Tag != null)
            {
                file.Tag.Pictures = pictures;
                file.Tag.Title = title;
                file.Tag.Performers = new[] { artist };
                file.Tag.Album = album;
                file.Save();
            }
            else
            {
                statusBar.Text = "File.Tag is null. Cannot assign pictures.";
            }
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
                SpotifyAPI.Web.Image albumArt = track.Album.Images[0];
                await previewWindow.SpotifyTrackPreview(ID, SpotifyTrackType.Track, ClientID(), ClientSecret());
                await Task.Delay(3000);
                if(GlobalVariables.SpotWindowClicked == true)
                {
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
                    string thumbPath = dlg.ResultPath + "\\thumbnail.png";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(new Uri(albumArt.Url), thumbPath);
                    }

                    string filepathS = $"{dlg.ResultPath}\\{artist} - {song}{fileFormat}";

                    statusBar.Text = "Downloading...";
                    await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathS).Build());
                    MergeThumbnailWithSong(filepathS, thumbPath, song, artist, album);
                    statusBar.Text = $"Downloaded  {song} Successfully!";
                    MessageBox.Show($"Downloaded {song} successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (System.IO.File.Exists(thumbPath))
                    {
                        System.IO.File.Delete(thumbPath);
                    }
                    GlobalVariables.SpotWindowClicked = false;
                }
                
            }
            else if(spotifyType == SpotifyType.Album)
            {
                //Get album
                var album = await spotify.Albums.Get(ID);
                var albumName = SanitizeFilename(album.Name);
                var artistA = album.Artists[0].Name;

                await previewWindow.SpotifyTrackPreview(ID, SpotifyTrackType.Album, ClientID(), ClientSecret());
                await Task.Delay(3000);

                if (GlobalVariables.SpotWindowClicked == true)
                {
                    statusBar.Text = "Retrieving Album Tracks...";


                    var dlg = new FolderPicker();
                    dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    if (dlg.ShowDialog() == true)
                    {
                        string filepath = dlg.ResultPath;
                    }

                    string albumDirectory = Path.Combine(dlg.ResultPath, albumName);
                    Directory.CreateDirectory(albumDirectory);
                    string filepathS = albumDirectory;

                    foreach (SimpleTrack item in album.Tracks.Items)
                    {
                        string trackID = item.Id;
                        var track = await spotify.Tracks.Get(trackID);
                        string song = SanitizeFilename(track.Name);
                        string artist = track.Artists[0].Name;
                        SpotifyAPI.Web.Image albumArt = track.Album.Images[0];
                        string albumA = SanitizeFilename(track.Album.Name);
                        string filepathD = $"{filepathS}\\{artist} - {song}{fileFormat}";

                        await previewWindow.SpotifyTrackPreview(trackID, SpotifyTrackType.Track, ClientID(), ClientSecret());
                        await Task.Delay(3000);

                        if (GlobalVariables.SpotWindowClicked == true)
                        {
                            statusBar.Text = "Retrieving Track...";
                            //YouTube song search and download
                            var result = await youtube.Search.GetVideosAsync(artist + " - " + song);
                            string videoID = result.First().Id;

                            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoID);
                            var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                            var streamInfo = new IStreamInfo[] { audioStream };

                            string thumbPath = filepathS + "\\thumbnail.png";
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(new Uri(albumArt.Url), thumbPath);
                            }

                            statusBar.Text = "Downloading...";
                            await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathD).Build());
                            MergeThumbnailWithSong(filepathD, thumbPath, song, artist, albumA);
                            statusBar.Text = $"Downloaded  {song} Successfully!";
                            MessageBox.Show($"Downloaded {song} successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                            if (System.IO.File.Exists(thumbPath))
                            {
                                System.IO.File.Delete(thumbPath);
                            }
                            GlobalVariables.SpotWindowClicked = false;
                        }       
                    }
                    statusBar.Text = $"Downloaded the album {albumName} successfully!";
                    MessageBox.Show($"Downloaded the album {albumName} Successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                }    
            }
            
            else if(spotifyType == SpotifyType.Playlist)
            {
                //Get playlist
                var playlist = await spotify.Playlists.Get(ID);
                var playlistName = SanitizeFilename(playlist.Name);
                var playlistAuthor = playlist.Owner;
                await previewWindow.SpotifyTrackPreview(ID, SpotifyTrackType.Playlist, ClientID(), ClientSecret());
                await Task.Delay(3000);

                if (GlobalVariables.SpotWindowClicked == true)
                {
                    statusBar.Text = "Retrieving Playlist Tracks...";

                    var dlg = new FolderPicker();
                    dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    if (dlg.ShowDialog() == true)
                    {
                        string filepath = dlg.ResultPath;
                    }

                    string playlistDirectory = Path.Combine(dlg.ResultPath, playlistName);
                    Directory.CreateDirectory(playlistDirectory);
                    string filepathS = playlistDirectory;

                    //Get playlist tracks            
                    foreach (PlaylistTrack<IPlayableItem> item in playlist.Tracks.Items)
                    {

                        if (item.Track is FullTrack track)
                        {
                            string trackID = track.Id;
                            var trackA = await spotify.Tracks.Get(trackID);
                            string song = SanitizeFilename(trackA.Name);
                            string artist = trackA.Artists[0].Name;
                            string albumA = SanitizeFilename(trackA.Album.Name);
                            SpotifyAPI.Web.Image albumArt = track.Album.Images[0];
                            string filepathD = $"{filepathS}\\{artist} - {song}{fileFormat}";
                            await previewWindow.SpotifyTrackPreview(trackID, SpotifyTrackType.Track, ClientID(), ClientSecret());
                            await Task.Delay(3000);

                            if (GlobalVariables.SpotWindowClicked == true)
                            {
                                statusBar.Text = "Retrieving Track...";
                                //YouTube song search and download
                                var result = await youtube.Search.GetVideosAsync(artist + " - " + song);
                                string videoID = result.First().Id;

                                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoID);
                                var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                                var streamInfo = new IStreamInfo[] { audioStream };
                                string thumbPath = filepathS + "\\thumbnail.png";
                                using (var client = new WebClient())
                                {
                                    client.DownloadFile(new Uri(albumArt.Url), thumbPath);
                                }

                                statusBar.Text = "Downloading...";
                                await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathD).Build());
                                MergeThumbnailWithSong(filepathD, thumbPath, song, artist, albumA);
                                statusBar.Text = $"Downloaded {song} from {playlistName} Successfully!";
                                MessageBox.Show($"Downloaded {song} from {playlistName} successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                if (System.IO.File.Exists(thumbPath))
                                {
                                    System.IO.File.Delete(thumbPath);
                                }
                                GlobalVariables.SpotWindowClicked = false;
                            }
                        }
                        if (item.Track is FullEpisode episode)
                        {
                            string trackID = episode.Id;
                            // All FullTrack properties are available
                            var trackA = await spotify.Episodes.Get(trackID);
                            string episodeName = trackA.Name;
                            string artist = trackA.Show.Publisher;
                            SpotifyAPI.Web.Image albumArt = trackA.Images[0];
                            string filepathD = $"{filepathS}\\{episodeName}{fileFormat}";
                            await previewWindow.SpotifyTrackPreview(trackID, SpotifyTrackType.Episode, ClientID(), ClientSecret());
                            await Task.Delay(3000);

                            if (GlobalVariables.SpotWindowClicked == true)
                            {
                                statusBar.Text = "Retrieving Track...";
                                //YouTube song search and download
                                var result = await youtube.Search.GetVideosAsync(episodeName);
                                string videoID = result.First().Id;

                                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoID);
                                var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                                var streamInfo = new IStreamInfo[] { audioStream };
                                string thumbPath = filepathS + "\\thumbnail.png";
                                using (var client = new WebClient())
                                {
                                    client.DownloadFile(new Uri(albumArt.Url), thumbPath);
                                }

                                statusBar.Text = "Downloading...";
                                await youtube.Videos.DownloadAsync(streamInfo, new ConversionRequestBuilder(filepathD).Build());
                                MergeThumbnailWithSong(filepathD, thumbPath, episodeName, artist, playlistName);
                                statusBar.Text = $"Downloaded {episodeName} from {playlistName} Successfully!";
                                MessageBox.Show($"Downloaded {episodeName} from {playlistName} successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                if (System.IO.File.Exists(thumbPath))
                                {
                                    System.IO.File.Delete(thumbPath);
                                }
                                GlobalVariables.SpotWindowClicked = false;
                            }
                           
                        }
                    }
                    statusBar.Text = $"Downloaded the playlist {playlistName} successfully!";
                    MessageBox.Show($"Downloaded the playlist {playlistName} Successfully!", "Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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
