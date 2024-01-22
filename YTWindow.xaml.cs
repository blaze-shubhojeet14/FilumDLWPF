using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using YoutubeExplode.Exceptions;
using static FilumDLWPF.FolderPicker;
using YoutubeExplode.Common;
using System.Threading.Tasks;
using System.Net.Http;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for YTWindow.xaml
    /// Window for the YouTube Downloader
    /// Copyright: Blaze Devs 2022-2024, All Rights Reserved
    /// </summary>
    public partial class YTWindow : Window
    {
        public YTWindow()
        {
            var AppName = GlobalVariables.AppName;
            var AppVersion = GlobalVariables.AppVersion;
            this.Title = $" {AppName} {AppVersion} - YouTube Downloader";
            InitializeComponent();
        }

        public enum YTVideoDLType
        {
            AudioAndVideo = 0,
            AudioOnly = 1,
            VideoOnly = 2
        }
        


        public string regexPattern = @"(?:https?:\/\/)?(?:www\.)?(?:m\.)?(?:youtube\.com\/(?:watch\?(?:.*&)?v=|v\/|embed\/|playlist\?(?:.*&)?list=|user\/\w+\/playlist\/)|youtu\.be\/|youtube.com\/shorts\/)([a-zA-Z0-9-_]+)";

        public string SanitizeFilename(string input)
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
        public string DownloadFormatSet()
        {
            if(ffInfoOpts.SelectedItem == FFavi)
            {
                return ".avi";
            }
            else if(ffInfoOpts.SelectedItem == FFmp3)
            {
                return ".mp3";
            }
            else if(ffInfoOpts.SelectedItem == FFmp4)
            {
                return ".mp4";
            }
            else
            {
                return ".mp4";
            }
        }
        public string DownloadResSet()
        {
            var des = dnRes.SelectedItem;
            if(des == p144)
            {
                return "144p";
            }
            else if(des == p240)
            {
                return "240p";
            }
            else if(des == p360)
            {
                return "360p";
            }
            else if(des == p480)
            {
                return "480p";
            }
            else if(des == p720)
            {
                return "720p";
            }
            else if(des == p1080)
            {
                return "1080p";
            }
            else if(des == p1440)
            {
                return "1440p";
            }
            else if(des == p2160)
            {
                return "2160p";
            }
            else
            {
                return "1080p";
            }
        }
        public string CCLanguageSet()
        {
            if(ccLang.SelectedItem == zh)
            {
                return "zh";
            }   
            else if(ccLang.SelectedItem == en)
            {
                return "en";
            }
            else if (ccLang.SelectedItem == ja)
            {
                return "ja";
            }
            else if (ccLang.SelectedItem == ko)
            {
                return "ko";
            }
            else if (ccLang.SelectedItem == es)
            {
                return "es";
            }
            else if (ccLang.SelectedItem == fr)
            {
                return "fr";
            }
            else if (ccLang.SelectedItem == de)
            {
                return "de";
            }
            else if (ccLang.SelectedItem == hi)
            {
                return "hi";
            }
            else if (ccLang.SelectedItem == bn)
            {
                return "bn";
            }
            else if (ccLang.SelectedItem == ru)
            {
                return "ru";
            }
            else if(ccLang.SelectedItem == ar)
            {
                return "ar";
            }
            else if(ccLang.SelectedItem == pt)
            {
                return "pt";
            }
            else if(ccLang.SelectedItem == tr)
            {
                return "tr";
            }
            else if(ccLang.SelectedItem == vi)
            {
                return "vi";
            }
            else if(ccLang.SelectedItem == pl)
            {
                return "pl";
            }
            else if(ccLang.SelectedItem == no)
            {
                return "no";
            }
            else if(ccLang.SelectedItem == ml)
            {
                return "ml";
            }
            else if(ccLang.SelectedItem == it)
            {
                return "it";
            }
            else if(ccLang.SelectedItem == id)
            {
                return "id";
            }
            else if(ccLang.SelectedItem == gu)
            {
                return "gu";
            }
            else if(ccLang.SelectedItem == asa)
            {
                return "as";
            }
            else
            {
                return "en";
            }
        }
        
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public async void YTVideoDownloadAsync(string dlURL, YTVideoDLType ytdlType, string captionLanguage, string fileFormat, string resFormat)
        {
            var youtubeClient = new YoutubeClient();
            var video = await youtubeClient.Videos.GetAsync(dlURL);
            
            string name = SanitizeFilename(video.Title);
            string channel = video.Author.ChannelTitle;
            var dlg = new FolderPicker();
            dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            if (dlg.ShowDialog() == true)
            {
                string filepath = dlg.ResultPath;
            }
            string filepathS = $"{dlg.ResultPath}\\{channel} - {name}{fileFormat}";
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(dlURL);
            if (ytdlType == YTVideoDLType.AudioAndVideo)
            {
                if (dnRes.SelectedItem == MaxQuality)
                {
                    var videoStreamInfo = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                    var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
      
                    if (dnCC.IsChecked == true)
                    {
                        var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                        var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                        if (trackInfo == null)
                        {
                            MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                            statusBar.Text = "No captions available for the selected language, proceeding with video download...";
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
                    var videoStreamInfoA = streamManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == resFormat);
                    var audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfosA = new IStreamInfo[] { audioStreamInfoA, videoStreamInfoA };
                    
                    if (dnCC.IsChecked == true)
                    {
                        var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                        var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                        if (trackInfo == null)
                        {
                            MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                            statusBar.Text = "No captions available for the selected language, proceeding with video download...";
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
            else if (ytdlType == YTVideoDLType.AudioOnly)
            {
                if (dnRes.SelectedItem == MaxQuality)
                {
                    var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfos = new IStreamInfo[] { audioStreamInfo };
                    var saveFileDialog = new SaveFileDialog();

                    if (dnCC.IsChecked == true)
                    {
                        var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                        var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                        if (trackInfo == null)
                        {
                            MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                            statusBar.Text = "No captions available for the selected language, proceeding with video download...";
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
                    MessageBox.Show("Downloaded the audio successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                { 
                    var audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfosA = new IStreamInfo[] { audioStreamInfoA };
                    
                    if (dnCC.IsChecked == true)
                    {
                        var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                        var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                        if (trackInfo == null)
                        {
                            MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                            statusBar.Text = "No captions available for the selected language, proceeding with video download...";
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
                        MessageBox.Show("Selected video quality unavilable, changing the resolution to highest audio quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                        audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                        streamInfosA = new IStreamInfo[] { audioStreamInfoA };

                        statusBar.Text = "Downloading...";
                        await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepathS).Build());

                        MessageBox.Show("Downloaded the audio successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        statusBar.Text = "Downloaded Successfully!";
                    }
                }
            }
            else if (ytdlType == YTVideoDLType.VideoOnly)
            {
                if (dnRes.SelectedItem == MaxQuality)
                {
                    var videoStreamInfo = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                    var streamInfos = new IStreamInfo[] { videoStreamInfo };
                 
                    if (dnCC.IsChecked == true)
                    {
                        var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                        var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                        if (trackInfo == null)
                        {
                            MessageBox.Show("No captions available for the selected language, proceeding with video download..., proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                            statusBar.Text = "No captions available for the selected language, proceeding with video download..., proceeding with video download...";
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
                    var videoStreamInfoA = streamManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == resFormat);
                    var streamInfosA = new IStreamInfo[] { videoStreamInfoA };
                   
                    if (dnCC.IsChecked == true)
                    {
                        var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(dlURL);
                        var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                        if (trackInfo == null)
                        {
                            MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                            statusBar.Text = "No captions available for the selected language, proceeding with video download...";
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
        }
        public async void YTPlaylistDownloadAsync(string dlURL, YTVideoDLType ytdlType, string captionLanguage, string fileFormat, string resFormat)
        {
            var youtubeClient = new YoutubeClient();
            if (ytdlType == YTVideoDLType.AudioAndVideo)
            {
                var dlg = new FolderPicker();
                dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (dlg.ShowDialog() == true)
                {
                    string filepath = dlg.ResultPath;
                }
                string filepathS = dlg.ResultPath;
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
                {
                    string name = SanitizeFilename(videoP.Title);
                    string channel = videoP.Author.ChannelTitle;
                    string filepath = $"{filepathS}\\{channel} - {name}{fileFormat}";

                    var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                    if (dnRes.SelectedItem == MaxQuality)
                    {
                        var audioStreamInfo = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                        var videoStreamInfo = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                        var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

                        if (dnCC.IsChecked == true)
                        {
                            var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                            if (trackInfo == null)
                            {
                                MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "No captions available for the selected language, proceeding with video download...";
                            }
                            else
                            {
                                statusBar.Text = "Downloading video captions...";
                                await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepath.Remove(filepath.Length - 4, 4) + ".srt");
                                statusBar.Text = "Downloaded successfully, proceeding with video download...";
                                MessageBox.Show("Downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        statusBar.Text = "Downloading...";
                        await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filepath).Build());
                        statusBar.Text = $"Downloaded  {filepath} Successfully!";
                        MessageBox.Show($"Downloaded {filepath} successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    else
                    {
                        var videoStreamInfoA = streamsManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == resFormat);
                        if (videoStreamInfoA == null)
                        {

                            videoStreamInfoA = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                        }
                        var audioStreamInfoA = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                        var streamInfosA = new IStreamInfo[] { audioStreamInfoA, videoStreamInfoA };
                        if (dnCC.IsChecked == true)
                        {
                            var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                            if (trackInfo == null)
                            {
                                MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "No captions available for the selected language, proceeding with video download...";
                            }
                            else
                            {
                                statusBar.Text = "Downloading video captions...";
                                await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepath.Remove(filepath.Length - 4, 4) + ".srt");
                                statusBar.Text = "Captions downloaded successfully, proceeding with video download...";
                            }
                        }
                        try
                        {
                            statusBar.Text = "Downloading...";
                            await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepath).Build());
                            statusBar.Text = $"Downloaded  {filepath} Successfully!";
                            MessageBox.Show($"Downloaded {filepath} successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (NullReferenceException)
                        {
                            MessageBox.Show("Selected video quality unavilable, changing the resolution to highest video quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                            videoStreamInfoA = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                            audioStreamInfoA = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                            streamInfosA = new IStreamInfo[] { audioStreamInfoA, videoStreamInfoA };

                            statusBar.Text = "Downloading...";
                            await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepath).Build());

                            statusBar.Text = "Downloaded Successfully!";
                            MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                    }
                    MessageBox.Show("Downloaded the playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    statusBar.Text = "Downloaded the playlist successfully!";
                }
            }
            else if (ytdlType == YTVideoDLType.AudioOnly)
            {
                var dlg = new FolderPicker();
                dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (dlg.ShowDialog() == true)
                {
                    string filepath = dlg.ResultPath;
                }
                string filepathS = dlg.ResultPath;
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
                {

                    string name = SanitizeFilename(videoP.Title);
                    string channel = videoP.Author.ChannelTitle;
                    string filepath = $"{filepathS}\\{channel} - {name}{fileFormat}";
                    var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                    if (dnRes.SelectedItem == MaxQuality)
                    {
                        var audioStreamInfo = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                        var streamInfos = new IStreamInfo[] { audioStreamInfo };

                        if (dnCC.IsChecked == true)
                        {
                            var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                            if (trackInfo == null)
                            {
                                MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "No captions available for the selected language, proceeding with video download...";
                            }
                            else
                            {
                                statusBar.Text = "Downloading video captions...";
                                await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepath.Remove(filepath.Length - 4, 4) + ".srt");
                                statusBar.Text = "Downloaded successfully, proceeding with audio download...";
                                MessageBox.Show("Downloaded successfully, proceeding with audio download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        statusBar.Text = "Downloading...";
                        await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filepath).Build());
                        statusBar.Text = $"Downloaded {filepath} Successfully!";
                        MessageBox.Show($"Downloaded {filepath} successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    else
                    {
                        var audioStreamInfoA = streamsManifest.GetAudioStreams().GetWithHighestBitrate();
                        var streamInfosA = new IStreamInfo[] { audioStreamInfoA };
                        if (dnCC.IsChecked == true)
                        {
                            var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                            if (trackInfo == null)
                            {
                                MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "No captions available for the selected language, proceeding with video download...";
                            }
                            else
                            {
                                statusBar.Text = "Downloading audio captions...";
                                await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepath.Remove(filepath.Length - 4, 4) + ".srt");
                                statusBar.Text = "Captions downloaded successfully, proceeding with audio download...";
                            }
                        }
                        try
                        {
                            statusBar.Text = "Downloading...";
                            await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepath).Build());
                            statusBar.Text = $"Downloaded {filepath} Successfully!";
                            MessageBox.Show($"Downloaded {filepath} successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch(Exception ex)
                        {
                            statusBar.Text = ex.Message;
                        }

                    }
                }
                MessageBox.Show("Downloaded the playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                statusBar.Text = "Downloaded the playlist successfully!";
            }
            else if (ytdlType == YTVideoDLType.VideoOnly)
            {
                var dlg = new FolderPicker();
                dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (dlg.ShowDialog() == true)
                {
                    string filepath = dlg.ResultPath;
                }
                string filepathS = dlg.ResultPath;
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
                {

                    string name = SanitizeFilename(videoP.Title);
                    string channel = videoP.Author.ChannelTitle;
                    string filepath = $"{filepathS}\\{channel} - {name}{fileFormat}";
                    var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                    if (dnRes.SelectedItem == MaxQuality)
                    {
                        var videoStreamInfo = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                        var streamInfos = new IStreamInfo[] { videoStreamInfo };

                        if (dnCC.IsChecked == true)
                        {
                            var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                            if (trackInfo == null)
                            {
                                MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "No captions available for the selected language, proceeding with video download...";
                            }
                            else
                            {
                                statusBar.Text = "Downloading video captions...";
                                await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepath.Remove(filepath.Length - 4, 4) + ".srt");
                                statusBar.Text = "Downloaded successfully, proceeding with video download...";
                                MessageBox.Show("Downloaded successfully, proceeding with video download...", "Captions Downloaded Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        statusBar.Text = "Downloading...";
                        await youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(filepath).Build());
                        statusBar.Text = $"Downloaded {filepath} Successfully!";
                    }
                    else
                    {
                        var videoStreamInfoA = streamsManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == resFormat);
                        var streamInfosA = new IStreamInfo[] { videoStreamInfoA };
                        if (dnCC.IsChecked == true)
                        {
                            var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoP.Id);
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                            if (trackInfo == null)
                            {
                                MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "No captions available for the selected language, proceeding with video download...";
                            }
                            else
                            {
                                statusBar.Text = "Downloading video captions...";
                                await youtubeClient.Videos.ClosedCaptions.DownloadAsync(trackInfo, filepath.Remove(filepath.Length - 4, 4) + ".srt");
                                statusBar.Text = "Captions downloaded successfully, proceeding with video download...";
                            }
                        }
                        try
                        {
                            statusBar.Text = "Downloading...";
                            await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepath).Build());
                            statusBar.Text = $"Downloaded {filepath} Successfully!";
                            MessageBox.Show($"Downloaded {filepath} successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (NullReferenceException)
                        {
                            MessageBox.Show("Selected video quality unavilable, changing the resolution to highest video quality...", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                            videoStreamInfoA = streamsManifest.GetVideoStreams().GetWithHighestVideoQuality();
                            streamInfosA = new IStreamInfo[] { videoStreamInfoA };

                            statusBar.Text = "Downloading...";
                            await youtubeClient.Videos.DownloadAsync(streamInfosA, new ConversionRequestBuilder(filepath).Build());

                            statusBar.Text = "Downloaded Successfully!";
                            MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                    }
                }
                MessageBox.Show("Downloaded the playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                statusBar.Text = "Downloaded the playlist successfully!";
            }
        }
        private async void dnButton_Click(object sender, RoutedEventArgs e)
        {
            VideoWindow videoWindow = new VideoWindow();

            var regex = new Regex(regexPattern);
            var match = regex.Match(dnURI.Text);
            if (match.Success)
              {
                  string videoId = match.Groups[1].Value;
              }

            string dlId = match.Groups[1].Value;

            
            try
            {
                    if (dnType.SelectedItem == Video)
                    {
                        bool? msgResult = MessageBoxCustom.ShowDialogBox("Do you want to preview the video before downloading?", "Preview Video", MessageBoxCustom.MessageBoxType.Question);
                        if (msgResult == true)
                        {
                           videoWindow.VideoPlayer(dlId);
                           await Task.Delay(40000);

                           if (audioVideo.IsSelected == true)
                           {
                                YTVideoDownloadAsync(dlId, YTVideoDLType.AudioAndVideo, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());
                           }
                           else if (audioOnly.IsSelected == true)
                           {
                                YTVideoDownloadAsync(dlId, YTVideoDLType.AudioOnly, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());

                           }
                           else if (videoOnly.IsSelected == true)
                           {
                                YTVideoDownloadAsync(dlId, YTVideoDLType.VideoOnly, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());
                           }
                        }
                        else if (msgResult == false)
                        {
                           if (audioVideo.IsSelected == true)
                           {
                                YTVideoDownloadAsync(dlId, YTVideoDLType.AudioAndVideo, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());
                           }
                           else if (audioOnly.IsSelected == true)
                           {
                                YTVideoDownloadAsync(dlId, YTVideoDLType.AudioOnly, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());

                           }
                           else if (videoOnly.IsSelected == true)
                           {
                                YTVideoDownloadAsync(dlId, YTVideoDLType.VideoOnly, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());
                           }

                        }
                    }                  
                    
                    else if (dnType.SelectedItem == Playlist)
                    {
                        if (audioVideo.IsSelected == true)
                        {
                            YTPlaylistDownloadAsync(dlId, YTVideoDLType.AudioAndVideo, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());
                        }
                        else if (audioOnly.IsSelected == true)
                        {
                            YTPlaylistDownloadAsync(dlId, YTVideoDLType.AudioOnly, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());

                        }
                        else if (videoOnly.IsSelected == true)
                        {
                            YTPlaylistDownloadAsync(dlId, YTVideoDLType.VideoOnly, CCLanguageSet(), DownloadFormatSet(), DownloadResSet());
                        }
                    }

                
            }
            catch (YoutubeExplodeException ex)
            {
                statusBar.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                statusBar.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                //For restarting the app, the code below is required:-
                //Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                //Application.Current.Shutdown();
            }
        }

        private void dnURI_TextChanged(object sender, TextChangedEventArgs e)
        {   
            if (dnType.SelectedItem == Video)
            {
                if (dnURI.Text != "")
                {
                    placeholder.Visibility = Visibility.Hidden;
                }
                else
                {
                    placeholder.Visibility = Visibility.Visible;
                }
                string regexPattern = @"(?:https?:\/\/)?(?:www\.)?(?:m\.)?(?:youtube\.com\/(?:watch\?(?:.*&)?v=|v\/|embed\/|shorts\/)|youtu\.be\/|youtube.com\/shorts\/)([a-zA-Z0-9-_]+)";
                var regex = new Regex(regexPattern);
                var match = regex.Match(dnURI.Text);
                if (match.Success)
                {
                    dnResLabel.Content = "Video Resolution:";
                    dnResDef.Content = "Choose the video resolution...";
                    dnResLabel.Visibility = Visibility.Visible;
                    dnRes.Visibility = Visibility.Visible;
                }
                else
                {
                    dnResLabel.Content = "Video Resolution:";
                    dnResDef.Content = "Choose the video resolution...";
                    dnResLabel.Visibility = Visibility.Hidden;
                    dnRes.Visibility = Visibility.Hidden;
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
                string regexPattern = @"(?:https?:\/\/)?(?:www\.)?(?:m\.)?(?:youtube\.com\/(?:playlist\?(?:.*&)?list=|embed\/|user\/\w+\/playlist\/)|youtu\.be\/)([a-zA-Z0-9-_]+)";

                var regex = new Regex(regexPattern);
                var match = regex.Match(dnURI.Text);
                if (match.Success)
                {
                    dnResLabel.Content = "Playlist Resolution:";
                    dnResDef.Content = "Choose the playlist videos resolution...";
                    dnResLabel.Visibility = Visibility.Visible;
                    dnRes.Visibility = Visibility.Visible;
                }
                else
                {
                    dnResLabel.Content = "Playlist Resolution:";
                    dnResDef.Content = "Choose the playlist videos resolution...";
                    dnResLabel.Visibility = Visibility.Hidden;
                    dnRes.Visibility = Visibility.Hidden;
                }
                
            }
        }
        private void dnOpts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dnOpts.SelectedItem == audioVideo)
            {
                if (ffInfoOpts.Items.Contains(FFmp3) == true)
                {
                    ffInfoOpts.Items.Remove(FFmp3);
                }
                
                if (ffInfoOpts.Items.Contains(FFavi) == false)
                {
                    ffInfoOpts.Items.Add(FFavi);
                }

                if (ffInfoOpts.Items.Contains(FFmp4) == false)
                {
                    ffInfoOpts.Items.Add(FFmp4);
                }

                if (dnType.SelectedItem == Playlist)
                {
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                    dnCC.Content = "Do you want to download video captions?";
                }
                else if (dnType.SelectedItem == Video)
                {
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                    dnCC.Content = "Do you want to download video captions?";
                }
            }
            else if(dnOpts.SelectedItem == videoOnly)
            {
                if (ffInfoOpts.Items.Contains(FFmp3) == true)
                {
                    ffInfoOpts.Items.Remove(FFmp3);
                }

                if (ffInfoOpts.Items.Contains(FFavi) == false)
                {
                    ffInfoOpts.Items.Add(FFavi);
                }

                if (ffInfoOpts.Items.Contains(FFmp4) == false)
                {
                    ffInfoOpts.Items.Add(FFmp4);
                }

                if (dnType.SelectedItem == Playlist)
                {
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                    dnCC.Content = "Do you want to download video captions?";
                }
                else if (dnType.SelectedItem == Video)
                {
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                    dnCC.Content = "Do you want to download video captions?";
                }
            }
            else if(dnOpts.SelectedItem == audioOnly)
            {
                if (ffInfoOpts.Items.Contains(FFmp3) == false)
                {
                    ffInfoOpts.Items.Add(FFmp3);
                }

                if (ffInfoOpts.Items.Contains(FFavi) == true)
                {
                    ffInfoOpts.Items.Remove(FFavi);
                }

                if (ffInfoOpts.Items.Contains(FFmp4) == true)
                {
                    ffInfoOpts.Items.Remove(FFmp4);
                }

                if (dnType.SelectedItem == Playlist)
                {
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                    dnCC.Content = "Do you want to download video captions?";
                }
                else if (dnType.SelectedItem == Video)
                {
                    ffInfoLabel.Visibility = Visibility.Visible;
                    ffInfoOpts.Visibility = Visibility.Visible;
                    dnCC.Content = "Do you want to download video captions?";
                }
            }
             

        }
        private void dnCC_Checked(object sender, RoutedEventArgs e)
        {
            ccLang.Visibility = Visibility.Visible;
            dnButton.Visibility = Visibility.Hidden;
        }
        

        private void dnRes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                if (dnType.SelectedItem == Video)
                {
                    dnOpts.Visibility = Visibility.Visible;
                    dnOptsLabel.Visibility = Visibility.Visible;
                }
                else if (dnType.SelectedItem == Playlist)
                {
                    dnOpts.Visibility = Visibility.Visible;
                    dnOptsLabel.Visibility = Visibility.Visible;
                }
        }   

        private void dnType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
                if (dnType.SelectedItem == Video)
                {
                    dnURILabel.Content = "Video URL:";
                    placeholder.Content = "Enter the video URL...";
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
        }

        private void ffInfoOpts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dnType.SelectedItem == Video)
            {
                dnCC.Visibility = Visibility.Visible;
                dnButton.Content = "Download";
                if (dnCC.IsChecked == false)
                {
                    ccLang.Visibility = Visibility.Hidden;
                    dnButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ccLang.Visibility = Visibility.Visible;
                    dnButton.Visibility = Visibility.Hidden;
                }
            }
            else if (dnType.SelectedItem == Playlist)
            {
                dnCC.Visibility = Visibility.Visible;
                dnButton.Content = "Download All";
                if (dnCC.IsChecked == false)
                {
                    ccLang.Visibility = Visibility.Hidden;
                    dnButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ccLang.Visibility = Visibility.Visible;
                    dnButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private void RMM_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void cc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dnButton.Visibility = Visibility.Visible;
            }
            catch (NullReferenceException)
            {
                if (dnType.SelectedItem == Video)
                {
                    dnButton.Content = "Download";
                }
                else if (dnType.SelectedItem == Playlist)
                {
                    dnButton.Content = "Download All";
                }
            }

        }
    }
}
