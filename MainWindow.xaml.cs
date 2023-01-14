using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void ListBoxVideo_Selected(object sender, RoutedEventArgs e)
        {
            dnURILabel.Content = "Video URL:";
            placeholder.Content = "Enter the video URL...";
            dnURILabel.Visibility = Visibility.Visible;
            dnURI.Visibility = Visibility.Visible;
            placeholder.Visibility = Visibility.Visible;
            dnURI.Text = "";
            dnResLabel.Content = "Video Resolution:";
            dnResDef.Content = "Choose the video resolution...";
            dnResLabel.Visibility = Visibility.Visible;
            dnRes.Visibility = Visibility.Visible;
            dnOptsLabel.Visibility = Visibility.Visible;
            dnOpts.Visibility = Visibility.Visible;

        }
        private void ListBoxPlaylist_Selected(object sender, RoutedEventArgs e)
        {
            dnURILabel.Content = "Playlist URL:";
            placeholder.Content = "Enter the playlist URL...";
            dnURILabel.Visibility = Visibility.Visible;
            dnURI.Visibility = Visibility.Visible;
            dnURI.Text = "";
            placeholder.Visibility = Visibility.Visible;
            dnResLabel.Content = "Playlist Resolution:";
            dnResDef.Content = "Choose the playlist videos resolution...";
            dnResLabel.Visibility = Visibility.Visible;
            dnRes.Visibility = Visibility.Visible;
            dnOptsLabel.Visibility = Visibility.Visible;
            dnOpts.Visibility = Visibility.Visible;

        }

        private void ListBoxYT_Selected(object sender, RoutedEventArgs e)
        {
            dnTypeLabel.Visibility = Visibility.Visible;
            dnType.Visibility = Visibility.Visible;
        }

        
        private async void dnButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var youtubeClient = new YoutubeClient();
                if (audioVideo.IsSelected == true)
                {
                    if (dnType.SelectedItem == Video)
                    {
                        string dlURL = dnURI.Text;
                        var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(dlURL);

                        if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                        {
                            var videoStreamInfo = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                            var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                            var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
                            var saveFileDialog = new SaveFileDialog();

                            saveFileDialog.Filter = "Video Files|*.mp4";
                            Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                            if (filepathDialog == true)
                            {
                                string filepath = saveFileDialog.FileName;
                            }
                            string filepathS = saveFileDialog.FileName;
                            if (dnCC.IsChecked == true)
                            {
                                var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                                var trackInfo = trackManifest.TryGetByLanguage("en");
                                if (trackInfo == null)
                                {
                                    MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                    statusBar.Text = "No english captions available, proceeding with video download...";
                                }
                                else
                                {
                                    statusBar.Text = "Downloading video captions...";
                                    await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepathS.Remove(filepathS.Length - 4, 4) + ".srt");
                                    statusBar.Text = "Downloaded successfully, proceeding with video download...";
                                    MessageBox.Show("Downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }

                            statusBar.Text = "Downloading...";
                            await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filepathS).Build());
                            statusBar.Text = "Downloaded Successfully!";
                        }
                        else
                        {
                            string dlRes = dnRes.SelectedItem.ToString();
                            dlRes = dlRes.Remove(0, 38);
                            var videoStreamInfoA = streamManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == dlRes);
                            var audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                            var streamInfosA = new IStreamInfo[] { audioStreamInfoA, videoStreamInfoA };
                            var saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "Video Files|*.mp4";
                            Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                            if (filepathDialog == true)
                            {
                                string filepath = saveFileDialog.FileName;
                            }
                            string filepathS = saveFileDialog.FileName;
                            if (dnCC.IsChecked == true)
                            {
                                var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                                var trackInfo = trackManifest.TryGetByLanguage("en");
                                if (trackInfo == null)
                                {
                                    MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                    statusBar.Text = "No english captions available, proceeding with video download...";
                                }
                                else
                                {
                                    statusBar.Text = "Downloading video captions...";
                                    await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepathS.Remove(filepathS.Length - 4, 4) + ".srt");
                                    statusBar.Text = "Captions downloaded successfully, proceeding with video download...";
                                    MessageBox.Show("Captions downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            try
                            {
                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepathS).Build());

                                statusBar.Text = "Downloaded Successfully!";
                                MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (NullReferenceException)
                            {
                                MessageBox.Show("Selected video quality unavilable, changing the resolution to highest video quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                                videoStreamInfoA = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                                audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                                streamInfosA = new IStreamInfo[] { audioStreamInfoA, videoStreamInfoA };

                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepathS).Build());

                                MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "Downloaded Successfully!";
                            }
                        }
                    }
                    else if (dnType.SelectedItem == Playlist)
                    {
                        string userName = Environment.UserName;
                        string PlaylistURL = dnURI.Text;
                        var saveFileDialog = new SaveFileDialog();
                        saveFileDialog.ValidateNames = false;
                        saveFileDialog.CheckFileExists = false;
                        saveFileDialog.CheckPathExists = true;
                        saveFileDialog.Filter = "Video Files|*.mp4";
                        string filenameP;
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            filenameP = saveFileDialog.FileName;
                        }
                        filenameP = saveFileDialog.FileName;
                        string filenameT = filenameP;
                        var i = 1;
                        await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(PlaylistURL))
                        {

                            string filenameSeq = filenameT.Remove(filenameT.Length - 4, 4) + i;
                            i = i + 1;
                            filenameP = filenameSeq + ".mp4";
                            var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                            if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                            {
                                var audioStreamInfo = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                                var videoStreamInfo = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                                var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

                                if (dnCC.IsChecked == true)
                                {
                                    var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                                    var trackInfo = trackManifest.TryGetByLanguage("en");
                                    if (trackInfo == null)
                                    {
                                        MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                        statusBar.Text = "No english captions available, proceeding with video download...";
                                    }
                                    else
                                    {
                                        statusBar.Text = "Downloading video captions...";
                                        await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filenameP.Remove(filenameP.Length - 4, 4) + ".srt");
                                        statusBar.Text = "Downloaded successfully, proceeding with video download...";
                                        MessageBox.Show("Downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                }
                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filenameP).Build());
                                statusBar.Text = "Downloaded " + filenameP + " Successfully!";
                            }
                            else
                            {
                                string dlRes = dnRes.SelectedItem.ToString();
                                dlRes = dlRes.Remove(0, 38);
                                var videoStreamInfoA = streamsManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == dlRes);
                                if (videoStreamInfoA == null)
                                {

                                    videoStreamInfoA = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                                }
                                var audioStreamInfoA = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                                var streamInfosA = new IStreamInfo[] { audioStreamInfoA, videoStreamInfoA };
                                if (dnCC.IsChecked == true)
                                {
                                    var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                                    var trackInfo = trackManifest.TryGetByLanguage("en");
                                    if (trackInfo == null)
                                    {
                                        MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                        statusBar.Text = "No english captions available, proceeding with video download...";
                                    }
                                    else
                                    {
                                        statusBar.Text = "Downloading video captions...";
                                        await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filenameP.Remove(filenameP.Length - 4, 4) + ".srt");
                                        statusBar.Text = "Captions downloaded successfully, proceeding with video download...";
                                    }
                                }
                                try
                                {
                                    statusBar.Text = "Downloading...";
                                    await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filenameP).Build());
                                    statusBar.Text = "Downloaded " + filenameP + " Successfully!";
                                    MessageBox.Show("Downloaded " + filenameP + " successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                catch (NullReferenceException)
                                {
                                    MessageBox.Show("Selected video quality unavilable, changing the resolution to highest video quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                                    videoStreamInfoA = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                                    audioStreamInfoA = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                                    streamInfosA = new IStreamInfo[] { audioStreamInfoA, videoStreamInfoA };

                                    statusBar.Text = "Downloading...";
                                    await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filenameP).Build());

                                    statusBar.Text = "Downloaded Successfully!";
                                    MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                }
                            }
                        }
                        MessageBox.Show("Downloaded the playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        statusBar.Text = "Downloaded the playlist successfully!";
                    }
                }
                else if(audioOnly.IsSelected == true)
                {
                    if (dnType.SelectedItem == Video)
                    {
                        string dlURL = dnURI.Text;
                        var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(dlURL);

                        if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                        {
                            var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                            var streamInfos = new IStreamInfo[] { audioStreamInfo };
                            var saveFileDialog = new SaveFileDialog();

                            saveFileDialog.Filter = "Audio Files|*.mp3";
                            Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                            if (filepathDialog == true)
                            {
                                string filepath = saveFileDialog.FileName;
                            }
                            string filepathS = saveFileDialog.FileName;
                            if (dnCC.IsChecked == true)
                            {
                                var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                                var trackInfo = trackManifest.TryGetByLanguage("en");
                                if (trackInfo == null)
                                {
                                    MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                    statusBar.Text = "No english captions available, proceeding with video download...";
                                }
                                else
                                {
                                    statusBar.Text = "Downloading video captions...";
                                    await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepathS.Remove(filepathS.Length - 4, 4) + ".srt");
                                    statusBar.Text = "Downloaded successfully, proceeding with video download...";
                                    MessageBox.Show("Downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }

                            statusBar.Text = "Downloading...";
                            await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filepathS).Build());
                            statusBar.Text = "Downloaded Successfully!";
                        }
                        else
                        {
                            string dlRes = dnRes.SelectedItem.ToString();
                            dlRes = dlRes.Remove(0, 38);
                            var audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                            var streamInfosA = new IStreamInfo[] { audioStreamInfoA };
                            var saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "Audio Files|*.mp3";
                            Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                            if (filepathDialog == true)
                            {
                                string filepath = saveFileDialog.FileName;
                            }
                            string filepathS = saveFileDialog.FileName;
                            if (dnCC.IsChecked == true)
                            {
                                var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                                var trackInfo = trackManifest.TryGetByLanguage("en");
                                if (trackInfo == null)
                                {
                                    MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                    statusBar.Text = "No english captions available, proceeding with video download...";
                                }
                                else
                                {
                                    statusBar.Text = "Downloading video captions...";
                                    await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepathS.Remove(filepathS.Length - 4, 4) + ".srt");
                                    statusBar.Text = "Captions downloaded successfully, proceeding with video download...";
                                    MessageBox.Show("Captions downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            try
                            {
                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepathS).Build());

                                statusBar.Text = "Downloaded Successfully!";
                                MessageBox.Show("Downloaded the audio successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (NullReferenceException)
                            {
                                MessageBox.Show("Selected video quality unavilable, changing the resolution to highest video quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                                audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                                streamInfosA = new IStreamInfo[] { audioStreamInfoA };

                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepathS).Build());

                                MessageBox.Show("Downloaded the audio successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "Downloaded Successfully!";
                            }
                        }
                    }
                    else if (dnType.SelectedItem == Playlist)
                    {
                        string userName = Environment.UserName;
                        string PlaylistURL = dnURI.Text;
                        var saveFileDialog = new SaveFileDialog();
                        saveFileDialog.ValidateNames = false;
                        saveFileDialog.CheckFileExists = false;
                        saveFileDialog.CheckPathExists = true;
                        saveFileDialog.Filter = "Audio Files|*.mp3";
                        string filenameP;
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            filenameP = saveFileDialog.FileName;
                        }
                        filenameP = saveFileDialog.FileName;
                        string filenameT = filenameP;
                        var i = 1;
                        await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(PlaylistURL))
                        {

                            string filenameSeq = filenameT.Remove(filenameT.Length - 4, 4) + i;
                            i = i + 1;
                            filenameP = filenameSeq + ".mp3";
                            var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                            if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                            {
                                var audioStreamInfo = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                                var streamInfos = new IStreamInfo[] { audioStreamInfo };

                                if (dnCC.IsChecked == true)
                                {
                                    var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                                    var trackInfo = trackManifest.TryGetByLanguage("en");
                                    if (trackInfo == null)
                                    {
                                        MessageBox.Show("No english captions available, proceeding with audio download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                        statusBar.Text = "No english captions available, proceeding with audio download...";
                                    }
                                    else
                                    {
                                        statusBar.Text = "Downloading video captions...";
                                        await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filenameP.Remove(filenameP.Length - 4, 4) + ".srt");
                                        statusBar.Text = "Downloaded successfully, proceeding with audio download...";
                                        MessageBox.Show("Downloaded successfully, proceeding with audio download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                }
                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filenameP).Build());
                                statusBar.Text = "Downloaded " + filenameP + " Successfully!";
                            }
                            else
                            {
                                string dlRes = dnRes.SelectedItem.ToString();
                                dlRes = dlRes.Remove(0, 38);                               
                                var audioStreamInfoA = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                                var streamInfosA = new IStreamInfo[] { audioStreamInfoA };
                                if (dnCC.IsChecked == true)
                                {
                                    var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                                    var trackInfo = trackManifest.TryGetByLanguage("en");
                                    if (trackInfo == null)
                                    {
                                        MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                        statusBar.Text = "No english captions available, proceeding with video download...";
                                    }
                                    else
                                    {
                                        statusBar.Text = "Downloading audio captions...";
                                        await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filenameP.Remove(filenameP.Length - 4, 4) + ".srt");
                                        statusBar.Text = "Captions downloaded successfully, proceeding with audio download...";
                                    }
                                }
                                try
                                {
                                    statusBar.Text = "Downloading...";
                                    await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filenameP).Build());
                                    statusBar.Text = "Downloaded " + filenameP + " Successfully!";
                                    MessageBox.Show("Downloaded " + filenameP + " successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                catch (NullReferenceException)
                                {
                                    MessageBox.Show("Selected video quality unavilable, changing the resolution to highest video quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                                    audioStreamInfoA = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                                    streamInfosA = new IStreamInfo[] { audioStreamInfoA };

                                    statusBar.Text = "Downloading...";
                                    await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filenameP).Build());

                                    statusBar.Text = "Downloaded Successfully!";
                                    MessageBox.Show("Downloaded the audio successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                }
                            }
                        }
                        MessageBox.Show("Downloaded the playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        statusBar.Text = "Downloaded the playlist successfully!";
                    }
                }
                else if(videoOnly.IsSelected == true)
                {
                    if (dnType.SelectedItem == Video)
                    {
                        string dlURL = dnURI.Text;
                        var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(dlURL);

                        if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                        {
                            var videoStreamInfo = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                            var streamInfos = new IStreamInfo[] { videoStreamInfo };
                            var saveFileDialog = new SaveFileDialog();

                            saveFileDialog.Filter = "Video Files|*.mp4";
                            Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                            if (filepathDialog == true)
                            {
                                string filepath = saveFileDialog.FileName;
                            }
                            string filepathS = saveFileDialog.FileName;
                            if (dnCC.IsChecked == true)
                            {
                                var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                                var trackInfo = trackManifest.TryGetByLanguage("en");
                                if (trackInfo == null)
                                {
                                    MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                    statusBar.Text = "No english captions available, proceeding with video download...";
                                }
                                else
                                {
                                    statusBar.Text = "Downloading video captions...";
                                    await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepathS.Remove(filepathS.Length - 4, 4) + ".srt");
                                    statusBar.Text = "Downloaded successfully, proceeding with video download...";
                                    MessageBox.Show("Downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }

                            statusBar.Text = "Downloading...";
                            await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filepathS).Build());
                            statusBar.Text = "Downloaded Successfully!";
                        }
                        else
                        {
                            string dlRes = dnRes.SelectedItem.ToString();
                            dlRes = dlRes.Remove(0, 38);
                            var videoStreamInfoA = streamManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == dlRes);
                            var streamInfosA = new IStreamInfo[] { videoStreamInfoA };
                            var saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "Video Files|*.mp4";
                            Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                            if (filepathDialog == true)
                            {
                                string filepath = saveFileDialog.FileName;
                            }
                            string filepathS = saveFileDialog.FileName;
                            if (dnCC.IsChecked == true)
                            {
                                var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                                var trackInfo = trackManifest.TryGetByLanguage("en");
                                if (trackInfo == null)
                                {
                                    MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                    statusBar.Text = "No english captions available, proceeding with video download...";
                                }
                                else
                                {
                                    statusBar.Text = "Downloading video captions...";
                                    await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepathS.Remove(filepathS.Length - 4, 4) + ".srt");
                                    statusBar.Text = "Captions downloaded successfully, proceeding with video download...";
                                    MessageBox.Show("Captions downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            try
                            {
                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepathS).Build());

                                statusBar.Text = "Downloaded Successfully!";
                                MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (NullReferenceException)
                            {
                                MessageBox.Show("Selected video quality unavilable, changing the resolution to highest video quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                                videoStreamInfoA = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                                streamInfosA = new IStreamInfo[] { videoStreamInfoA };

                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepathS).Build());

                                MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "Downloaded Successfully!";
                            }
                        }
                    }
                    else if (dnType.SelectedItem == Playlist)
                    {
                        string userName = Environment.UserName;
                        string PlaylistURL = dnURI.Text;
                        var saveFileDialog = new SaveFileDialog();
                        saveFileDialog.ValidateNames = false;
                        saveFileDialog.CheckFileExists = false;
                        saveFileDialog.CheckPathExists = true;
                        saveFileDialog.Filter = "Video Files|*.mp4";
                        string filenameP;
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            filenameP = saveFileDialog.FileName;
                        }
                        filenameP = saveFileDialog.FileName;
                        string filenameT = filenameP;
                        var i = 1;
                        await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(PlaylistURL))
                        {

                            string filenameSeq = filenameT.Remove(filenameT.Length - 4, 4) + i;
                            i = i + 1;
                            filenameP = filenameSeq + ".mp4";
                            var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                            if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                            {
                                var videoStreamInfo = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                                var streamInfos = new IStreamInfo[] { videoStreamInfo };

                                if (dnCC.IsChecked == true)
                                {
                                    var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                                    var trackInfo = trackManifest.TryGetByLanguage("en");
                                    if (trackInfo == null)
                                    {
                                        MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                        statusBar.Text = "No english captions available, proceeding with video download...";
                                    }
                                    else
                                    {
                                        statusBar.Text = "Downloading video captions...";
                                        await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filenameP.Remove(filenameP.Length - 4, 4) + ".srt");
                                        statusBar.Text = "Downloaded successfully, proceeding with video download...";
                                        MessageBox.Show("Downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                }
                                statusBar.Text = "Downloading...";
                                await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filenameP).Build());
                                statusBar.Text = "Downloaded " + filenameP + " Successfully!";
                            }
                            else
                            {
                                string dlRes = dnRes.SelectedItem.ToString();
                                dlRes = dlRes.Remove(0, 38);
                                var videoStreamInfoA = streamsManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == dlRes);
                                var streamInfosA = new IStreamInfo[] { videoStreamInfoA };
                                if (dnCC.IsChecked == true)
                                {
                                    var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                                    var trackInfo = trackManifest.TryGetByLanguage("en");
                                    if (trackInfo == null)
                                    {
                                        MessageBox.Show("No english captions available, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                        statusBar.Text = "No english captions available, proceeding with video download...";
                                    }
                                    else
                                    {
                                        statusBar.Text = "Downloading video captions...";
                                        await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filenameP.Remove(filenameP.Length - 4, 4) + ".srt");
                                        statusBar.Text = "Captions downloaded successfully, proceeding with video download...";
                                    }
                                }
                                try
                                {
                                    statusBar.Text = "Downloading...";
                                    await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filenameP).Build());
                                    statusBar.Text = "Downloaded " + filenameP + " Successfully!";
                                    MessageBox.Show("Downloaded " + filenameP + " successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                catch (NullReferenceException)
                                {
                                    MessageBox.Show("Selected video quality unavilable, changing the resolution to highest video quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                                    videoStreamInfoA = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                                    streamInfosA = new IStreamInfo[] { videoStreamInfoA };

                                    statusBar.Text = "Downloading...";
                                    await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filenameP).Build());

                                    statusBar.Text = "Downloaded Successfully!";
                                    MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                }
                            }
                        }
                        MessageBox.Show("Downloaded the playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        statusBar.Text = "Downloaded the playlist successfully!";
                    }
                }
            }
            catch(YoutubeExplode.Exceptions.YoutubeExplodeException ex)
            {
                statusBar.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(Exception ex)
            {
                statusBar.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                //Application.Current.Shutdown();
            }
        }

        private void dnURI_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(dnURI.Text != "")
            {
                placeholder.Visibility = Visibility.Hidden;
            }
            else
            {
                placeholder.Visibility = Visibility.Visible;
            }
        }
        private void dnOpts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                if (dnType.SelectedItem == Playlist)
                {
                    dnCC.Content = "Do you want to download video captions for all videos too (English only)?";
                    dnCC.Visibility = Visibility.Visible;
                    dnButton.Content = "Download All";
                    dnButton.Visibility = Visibility.Visible;
                }
                else if (dnType.SelectedItem == Video)
                {
                    dnCC.Content = "Do you want to download video captions too (English only)?";
                    dnCC.Visibility = Visibility.Visible;
                    dnButton.Content = "Download";
                    dnButton.Visibility = Visibility.Visible;
                }
        }
    }
}
