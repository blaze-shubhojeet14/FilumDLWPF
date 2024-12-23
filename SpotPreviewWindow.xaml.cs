using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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
         
        public async Task SpotifyTrackPreview(string trackID, SpotifyTrackType spotifyTrackType)
        {
            try
            {
                var auth = new AuthenticationService().GetSpotifyClient();
                var TrackService = new TrackService(await auth);
                var AlbumService = new AlbumService(await auth);
                var PlaylistService = new PlaylistService(await auth);
                var EpisodeService = new EpisodeService(await auth);

                this.Closed += (s, e) => GlobalVariables.SpotWindowClicked = true;

                if (spotifyTrackType == SpotifyTrackType.Track)
                {
                    infoHeader.Content = "Track Information";
                    var track = await TrackService.GetTrack(trackID);
                    string song = TrackService.SongName;
                    string artist = TrackService.ArtistName;
                    string album = TrackService.AlbumName;

                    trackName.Content = "Track Name: " + song;
                    artistName.Content = "Artist Name: " + artist;
                    albumName.Content = "Album Name: " + album;
                    string coverUrl = TrackService.AlbumArt.Url;
                    coverArt.Source = new BitmapImage(new Uri(coverUrl));
                    await Task.Delay(3000);
                    this.Show();
                }
                else if(spotifyTrackType == SpotifyTrackType.Album)
                {
                    infoHeader.Content = "Album Information";
                    var album = await AlbumService.GetAlbum(trackID);
                    var albumNameA = AlbumService.AlbumName;
                    var artist = AlbumService.ArtistName;
                    trackName.Content = "Album Name: " + albumNameA;
                    artistName.Content = "Artist Name: " + artist;
                    albumName.Content = "";
                    string coverUrl = AlbumService.AlbumArt.Url;
                    coverArt.Source = new BitmapImage(new Uri(coverUrl));
                    await Task.Delay(3000);
                    this.Show();

                }
                else if(spotifyTrackType == SpotifyTrackType.Playlist)
                {
                    infoHeader.Content = "Playlist Information";
                    var playlist = await PlaylistService.GetPlaylist(trackID);
                    var playlistName = PlaylistService.PlaylistName;
                    var playlistAuthor = PlaylistService.PlaylistAuthor;
                    trackName.Content = "Playlist Name: " + playlistName;
                    artistName.Content = "Author Name: " + playlistAuthor;
                    albumName.Content = "";
                    string coverUrl = PlaylistService.PlaylistArt.Url;
                    coverArt.Source = new BitmapImage(new Uri(coverUrl));
                    await Task.Delay(3000);
                    this.Show();
                }
                else if(spotifyTrackType == SpotifyTrackType.Episode)
                {
                    infoHeader.Content = "Episode Information";
                    var episode = await EpisodeService.GetEpisode(trackID);
                    var episodeName = EpisodeService.EpisodeName;
                    var episodeAuthor = EpisodeService.EpisodeAuthor;
                    trackName.Content = "Episode Name: " + episodeName;
                    artistName.Content = "Publisher Name: " + episodeAuthor;
                    albumName.Content = "";
                    string coverUrl = EpisodeService.EpisodeArt.Url;
                    coverArt.Source = new BitmapImage(new Uri(coverUrl));
                    await Task.Delay(3000);
                    this.Show();
                }

            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                GlobalVariables.SpotWindowClicked = true;
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
