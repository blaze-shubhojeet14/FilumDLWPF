using SpotifyAPI.Web;
using System;
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
using static FilumDLWPF.SpotPreviewWindow;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for SpotWindow.xaml
    /// Window for the Spotify Downloader
    /// Copyright: Blaze Devs 2022-2024, All Rights Reserved
    /// </summary>


    // Spotify API Services
    // <start>
    public class FileSanitizer
    {
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
    }
    public interface ITrackService
    {
        Task<FullTrack> GetTrack(string id);
        String SongName { get; set; }
        String ArtistName { get; set; }
        String AlbumName { get; set; }
        SpotifyAPI.Web.Image AlbumArt { get; set; }     
    }
    public class TrackService : FileSanitizer, ITrackService
    {
        private readonly SpotifyClient _spotify;

        public TrackService(SpotifyClient spotify)
        {
            _spotify = spotify;
        }

        public async Task<FullTrack> GetTrack(string id)
        {
            var track = await _spotify.Tracks.Get(id);
            SongName = SanitizeFilename(track.Name);
            ArtistName = SanitizeFilename(track.Artists[0].Name);
            AlbumName = SanitizeFilename(track.Album.Name);
            AlbumArt = track.Album.Images[0];
            return track;                   
        }
        public String SongName { get; set; }
        public String ArtistName { get; set; }
        public String AlbumName { get; set; }
        public SpotifyAPI.Web.Image AlbumArt { get; set; }
    }

    public interface IAlbumService
    {
        Task<FullAlbum> GetAlbum(string id);
        String ArtistName { get; set; }
        String AlbumName { get; set; }
        SpotifyAPI.Web.Image AlbumArt { get; set; }
    }
    public class AlbumService : FileSanitizer, IAlbumService
    {
        private readonly SpotifyClient _spotify;

        public AlbumService(SpotifyClient spotify)
        {
            _spotify = spotify;
        }

        public async Task<FullAlbum> GetAlbum(string id)
        {
            var album = await _spotify.Albums.Get(id);
            ArtistName = SanitizeFilename(album.Artists[0].Name);
            AlbumName = SanitizeFilename(album.Name);
            AlbumArt = album.Images[0];
            return album;

        }
        public String ArtistName { get; set; }
        public String AlbumName { get; set; }
        public SpotifyAPI.Web.Image AlbumArt { get; set; }
    }
    public interface IPlaylistService
    {
        Task<FullPlaylist> GetPlaylist(string id);
        String PlaylistName { get; set; }
        String PlaylistAuthor { get; set; }
        SpotifyAPI.Web.Image PlaylistArt { get; set; }
    }
    public class PlaylistService : FileSanitizer, IPlaylistService
    {
        private readonly SpotifyClient _spotify;

        public PlaylistService(SpotifyClient spotify)
        {
            _spotify = spotify;
        }

        public async Task<FullPlaylist> GetPlaylist(string id)
        {
            var playlist = await _spotify.Playlists.Get(id);
            PlaylistName = SanitizeFilename(playlist.Name);
            PlaylistAuthor = playlist.Owner.DisplayName;
            PlaylistArt = playlist.Images[0];
            return playlist;
        }
        public String PlaylistName { get; set; }
        public String PlaylistAuthor { get; set; }
        public SpotifyAPI.Web.Image PlaylistArt { get; set; }
    }
    public interface IEpisodeService
    {
        Task<FullEpisode> GetEpisode(string id);
        String EpisodeName { get; set; }
        String EpisodeAuthor { get; set; }
        SpotifyAPI.Web.Image EpisodeArt { get; set; }
    }
    public class EpisodeService : FileSanitizer, IEpisodeService
    {
        private readonly SpotifyClient _spotify;

        public EpisodeService(SpotifyClient spotify)
        {
            _spotify = spotify;
        }

        public async Task<FullEpisode> GetEpisode(string id)
        {
            var episode = await _spotify.Episodes.Get(id);
            EpisodeName = SanitizeFilename(episode.Name);
            EpisodeAuthor = SanitizeFilename(episode.Show.Publisher);
            EpisodeArt = episode.Images[0];
            return episode;
        }
        public String EpisodeName { get; set; }
        public String EpisodeAuthor { get; set; }
        public SpotifyAPI.Web.Image EpisodeArt { get; set; }
    }

    public interface IAuthenticationService
    {
        Task<SpotifyClient> GetSpotifyClient();
    }
    public class AuthenticationService : IAuthenticationService
    {
        public async Task<SpotifyClient> GetSpotifyClient()
        {
            Secrets secrets = new Secrets();
            string clientID = secrets.SetSpotifyClientID();
            string clientSecret = secrets.SetSpotifyClientSecret();
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(clientID, clientSecret);
            var response = await new OAuthClient(config).RequestToken(request);
            if (response.IsExpired == true)
            {
                response = await new OAuthClient(config).RequestToken(request);
            }
            var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

            return spotify;
        }
    }
    // <end>



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

        WebClient client = new WebClient();

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
                file.Tag.AlbumArtists = new[] { artist };
                file.Tag.Publisher = "Spotify";
                file.Tag.Copyright = artist + " 2022-2024, All Rights Reserved";
                file.Tag.Description = $"Downloaded with Filum-DL Application {GlobalVariables.AppVersion} | Copyright: Blaze Devs 2022-2024, All Rights Reserved ";
                file.Save();
            }
            else
            {
                statusBar.Text = "File.Tag is null. Cannot assign pictures.";
            }
        }

        public async void GetMultipleTracks(string ID, SpotifyType spotifyType, string fileFormat)
        {
            //Authentication
            var auth = new AuthenticationService().GetSpotifyClient();
            var TrackService = new TrackService(await auth);
            var AlbumService = new AlbumService(await auth);
            var PlaylistService = new PlaylistService(await auth);
            var EpisodeService = new EpisodeService(await auth);

            //YouTube Client Configuration
            var youtube = new YoutubeClient();

            statusBar.Text = "Spotify client has been authenticated successfully";
            
            //Get song metadata
            if(spotifyType == SpotifyType.Track)
            {
                var track = await TrackService.GetTrack(ID);
                
                string song = TrackService.SongName;
                string artist = TrackService.ArtistName;
                string album = TrackService.AlbumName;
                var albumArt = TrackService.AlbumArt;

                await previewWindow.SpotifyTrackPreview(ID, SpotifyTrackType.Track);
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
                    using (client)
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
                var album = await AlbumService.GetAlbum(ID);
                var albumName = AlbumService.AlbumName;               

                await previewWindow.SpotifyTrackPreview(ID, SpotifyTrackType.Album);
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
                        var track = await TrackService.GetTrack(trackID);
                        var song = TrackService.SongName;
                        var artist = TrackService.ArtistName;
                        var albumArt = TrackService.AlbumArt;
                        string albumA = TrackService.AlbumName;

                        string filepathD = $"{filepathS}\\{artist} - {song}{fileFormat}";

                        await previewWindow.SpotifyTrackPreview(trackID, SpotifyTrackType.Track);
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
                            using (client)
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
                var playlist = await PlaylistService.GetPlaylist(ID);
                var playlistName = PlaylistService.PlaylistName;

                await previewWindow.SpotifyTrackPreview(ID, SpotifyTrackType.Playlist);
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
                            var trackA = await TrackService.GetTrack(trackID);
                            var song = TrackService.SongName;
                            var artist = TrackService.ArtistName;
                            var albumArt = TrackService.AlbumArt;
                            string albumA = TrackService.AlbumName;

                            string filepathD = $"{filepathS}\\{artist} - {song}{fileFormat}";
                            await previewWindow.SpotifyTrackPreview(trackID, SpotifyTrackType.Track);
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
                                using (client)
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
                            var trackA = await EpisodeService.GetEpisode(trackID);
                            var episodeName = EpisodeService.EpisodeName;
                            var artist = EpisodeService.EpisodeAuthor;
                            var albumArt = EpisodeService.EpisodeArt;

                            string filepathD = $"{filepathS}\\{episodeName}{fileFormat}";

                            await previewWindow.SpotifyTrackPreview(trackID, SpotifyTrackType.Episode);

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
                                using (client)
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
                    GetMultipleTracks(dlIdSong, SpotifyType.Track, FileFormatSet());
                }
                else if (dnType.SelectedItem == Playlist)
                {
                    GetMultipleTracks(dlIdPlaylist, SpotifyType.Playlist, FileFormatSet());
                }
                else if (dnType.SelectedItem == Album)
                {
                    GetMultipleTracks(dlIdAlbum, SpotifyType.Album, FileFormatSet());
                }
            }
            catch(Exception ex)
            {
                statusBar.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
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
