using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using Microsoft.Win32;
using System.IO;
using AngleSharp.Io;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using YoutubeExplode.Exceptions;
//using DotNetTools.SharpGrabber.Instagram;
//using DotNetTools.SharpGrabber;
//using DotNetTools.SharpGrabber.Converter;
//using DotNetTools.SharpGrabber.Grabbed;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for YTWindow.xaml
    /// </summary>
    public partial class YTWindow : Window
    {
        public YTWindow()
        {
            InitializeComponent();
        }

        public enum YTVideoDLType
        {
            AudioAndVideo = 0,
            AudioOnly = 1,
            VideoOnly = 2
        }
        
        //public IMultiGrabber grabber = GrabberBuilder.New().UseDefaultServices().AddHls().AddYouTube().AddVimeo().AddInstagram().Build();

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
            else
            {
                return "en";
            }
        }
        public async void YTVideoDownloadAsync(string dlURL, YTVideoDLType ytdlType, string captionLanguage, string fileFormat)
        {
            var youtubeClient = new YoutubeClient();
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(dlURL);
            if (ytdlType == YTVideoDLType.AudioAndVideo)
            {
                if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                {
                    var videoStreamInfo = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                    var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
                    var saveFileDialog = new SaveFileDialog();

                    if (fileFormat == ".avi")
                    {
                        saveFileDialog.Filter = "AVI Video (*.avi)|*.avi";
                    }
                    else if (fileFormat == ".mp4")
                    {
                        saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                    }
                    else
                    {
                        saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                    }

                    Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                    if (filepathDialog == true)
                    {
                        string filepath = saveFileDialog.FileName;
                    }
                    string filepathS = saveFileDialog.FileName;
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
                    string dlRes = dnRes.SelectedItem.ToString();
                    dlRes = dlRes.Remove(0, 38);
                    var videoStreamInfoA = streamManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == dlRes);
                    var audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfosA = new IStreamInfo[] { audioStreamInfoA, videoStreamInfoA };
                    var saveFileDialog = new SaveFileDialog();
                    if(fileFormat == ".avi")
                    {
                        saveFileDialog.Filter = "AVI Video (*.avi)|*.avi";
                    }
                    else if (fileFormat == ".mp4")
                    {
                        saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                    }
                    else
                    {
                        saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                    }
                    Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                    if (filepathDialog == true)
                    {
                        string filepath = saveFileDialog.FileName;
                    }
                    string filepathS = saveFileDialog.FileName;
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
                if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                {
                    var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfos = new IStreamInfo[] { audioStreamInfo };
                    var saveFileDialog = new SaveFileDialog();

                    if (fileFormat == ".mp3")
                    {
                        saveFileDialog.Filter = "MP3 Audio (*.mp3)|*.mp3";
                    }
                    else
                    {
                        saveFileDialog.Filter = "MP3 Audio (*.mp3)|*.mp3";
                    }
                    Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                    if (filepathDialog == true)
                    {
                        string filepath = saveFileDialog.FileName;
                    }
                    string filepathS = saveFileDialog.FileName;
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
                    string dlRes = dnRes.SelectedItem.ToString();
                    dlRes = dlRes.Remove(0, 38);
                    var audioStreamInfoA = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                    var streamInfosA = new IStreamInfo[] { audioStreamInfoA };
                    var saveFileDialog = new SaveFileDialog();
                    if (fileFormat == ".mp3")
                    {
                        saveFileDialog.Filter = "MP3 Audio (*.mp3)|*.mp3";
                    }
                    else
                    {
                        saveFileDialog.Filter = "MP3 Audio (*.mp3)|*.mp3";
                    }
                    Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                    if (filepathDialog == true)
                    {
                        string filepath = saveFileDialog.FileName;
                    }
                    string filepathS = saveFileDialog.FileName;
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
                if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
                {
                    var videoStreamInfo = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                    var streamInfos = new IStreamInfo[] { videoStreamInfo };
                    var saveFileDialog = new SaveFileDialog();

                    if (fileFormat == ".avi")
                    {
                        saveFileDialog.Filter = "AVI Video (*.avi)|*.avi";
                    }
                    else if (fileFormat == ".mp4")
                    {
                        saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                    }
                    else
                    {
                        saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                    }
                    Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                    if (filepathDialog == true)
                    {
                        string filepath = saveFileDialog.FileName;
                    }
                    string filepathS = saveFileDialog.FileName;
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
                    string dlRes = dnRes.SelectedItem.ToString();
                    dlRes = dlRes.Remove(0, 38);
                    var videoStreamInfoA = streamManifest.GetVideoStreams().FirstOrDefault(s => s.VideoQuality.Label == dlRes);
                    var streamInfosA = new IStreamInfo[] { videoStreamInfoA };
                    var saveFileDialog = new SaveFileDialog();
                    if (fileFormat == ".avi")
                    {
                        saveFileDialog.Filter = "AVI Video (*.avi)|*.avi";
                    }
                    else if (fileFormat == ".mp4")
                    {
                        saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                    }
                    else
                    {
                        saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                    }
                    Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                    if (filepathDialog == true)
                    {
                        string filepath = saveFileDialog.FileName;
                    }
                    string filepathS = saveFileDialog.FileName;
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
        public async void YTPlaylistDownloadAsync(string dlURL, YTVideoDLType ytdlType, string captionLanguage, string fileFormat)
        {
            var youtubeClient = new YoutubeClient();
            if (ytdlType == YTVideoDLType.AudioAndVideo)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.ValidateNames = false;
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.CheckPathExists = true;
                if (fileFormat == ".avi")
                {
                    saveFileDialog.Filter = "AVI Video (*.avi)|*.avi";
                }
                else if (fileFormat == ".mp4")
                {
                    saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                }
                else
                {
                    saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                }
                string filenameP;
                if (saveFileDialog.ShowDialog() == true)
                {
                    filenameP = saveFileDialog.FileName;
                }
                filenameP = saveFileDialog.FileName;
                string filenameT = filenameP;
                var i = 1;
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
                {

                    string filenameSeq = filenameT.Remove(filenameT.Length - 4, 4) + i;
                    i = i + 1;
                    filenameP = filenameSeq + fileFormat;
                    var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                    if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
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
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                            if (trackInfo == null)
                            {
                                MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "No captions available for the selected language, proceeding with video download...";
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
                    MessageBox.Show("Downloaded the playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    statusBar.Text = "Downloaded the playlist successfully!";
                }
            }
            else if (ytdlType == YTVideoDLType.AudioOnly)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.ValidateNames = false;
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.CheckPathExists = true;
                if (fileFormat == ".mp3")
                {
                    saveFileDialog.Filter = "MP3 Audio (*.mp3)|*.mp3";
                }
                else
                {
                    saveFileDialog.Filter = "MP3 Audio (*.mp3)|*.mp3";
                }
                string filenameP;
                if (saveFileDialog.ShowDialog() == true)
                {
                    filenameP = saveFileDialog.FileName;
                }
                filenameP = saveFileDialog.FileName;
                string filenameT = filenameP;
                var i = 1;
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
                {

                    string filenameSeq = filenameT.Remove(filenameT.Length - 4, 4) + i;
                    i = i + 1;
                    filenameP = filenameSeq + fileFormat;
                    var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                    if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        dlRes = dlRes.Remove(0, 38);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
            else if (ytdlType == YTVideoDLType.VideoOnly)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.ValidateNames = false;
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.CheckPathExists = true;
                if (fileFormat == ".avi")
                {
                    saveFileDialog.Filter = "AVI Video (*.avi)|*.avi";
                }
                else if (fileFormat == ".mp4")
                {
                    saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                }
                else
                {
                    saveFileDialog.Filter = "MP4 Video (*.mp4)|*.mp4";
                }
                string filenameP;
                if (saveFileDialog.ShowDialog() == true)
                {
                    filenameP = saveFileDialog.FileName;
                }
                filenameP = saveFileDialog.FileName;
                string filenameT = filenameP;
                var i = 1;
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
                {

                    string filenameSeq = filenameT.Remove(filenameT.Length - 4, 4) + i;
                    i = i + 1;
                    filenameP = filenameSeq + fileFormat;
                    var streamsManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoP.Id);

                    if (dnRes.SelectedItem.ToString().Remove(0, 38) == "Highest Video Quality")
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
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
                            if (trackInfo == null)
                            {
                                MessageBox.Show("No captions available for the selected language, proceeding with video download...", "Captions Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                                statusBar.Text = "No captions available for the selected language, proceeding with video download...";
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
        private void dnButton_Click(object sender, RoutedEventArgs e)
        {
            string dlURL = dnURI.Text;
            try
            {
                    if (dnType.SelectedItem == Video)
                    {
                        if (audioVideo.IsSelected == true)
                        {
                            YTVideoDownloadAsync(dlURL, YTVideoDLType.AudioAndVideo, CCLanguageSet(), DownloadFormatSet());
                        }
                        else if (audioOnly.IsSelected == true)
                        {
                            YTVideoDownloadAsync(dlURL, YTVideoDLType.AudioOnly, CCLanguageSet(), DownloadFormatSet());

                        }
                        else if (videoOnly.IsSelected == true)
                        {
                            YTVideoDownloadAsync(dlURL, YTVideoDLType.VideoOnly, CCLanguageSet(), DownloadFormatSet());
                        }
                    }
                    else if (dnType.SelectedItem == Playlist)
                    {
                        if (audioVideo.IsSelected == true)
                        {
                        YTPlaylistDownloadAsync(dlURL, YTVideoDLType.AudioAndVideo, CCLanguageSet(), DownloadFormatSet());
                    }
                        else if (audioOnly.IsSelected == true)
                        {
                            YTPlaylistDownloadAsync(dlURL, YTVideoDLType.AudioOnly, CCLanguageSet(), DownloadFormatSet());

                        }
                        else if (videoOnly.IsSelected == true)
                        {
                            YTPlaylistDownloadAsync(dlURL, YTVideoDLType.VideoOnly, CCLanguageSet(), DownloadFormatSet());
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
                dnResLabel.Content = "Video Resolution:";
                dnResDef.Content = "Choose the video resolution...";
                dnResLabel.Visibility = Visibility.Visible;
                dnRes.Visibility = Visibility.Visible;
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
                dnResLabel.Content = "Playlist Resolution:";
                dnResDef.Content = "Choose the playlist videos resolution...";
                dnResLabel.Visibility = Visibility.Visible;
                dnRes.Visibility = Visibility.Visible;
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

        private void ccLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dnButton.Visibility = Visibility.Visible;
        }

        private void dnCC_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
