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

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public enum TWVideoDLQuality
        {
            Highest = 0,
            Higher = 1,
            High = 2,
            m3u8 = 3
        }
        public enum YTVideoDLType
        {
            AudioAndVideo = 0,
            AudioOnly = 1,
            VideoOnly = 2
        }
        public async void TWVideoDownloadAsync(Uri uri, string bearerToken, string filePath, TWVideoDLQuality twVideoDLQuality)
        {
            try
            {
                var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, uri);
                request.Headers.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10136");
                request.Headers.Add(HeaderNames.Accept, "application/json");
                request.Headers.Add(HeaderNames.Authorization, bearerToken);

                var client = new HttpClient();
                var response = await client.SendAsync(request);
                var res = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject(res);

                JObject joResponse = JObject.Parse(json.ToString());
                JObject ojObject = (JObject)joResponse["extended_entities"];
                JArray media = (JArray)ojObject["media"];

                //Index values for the variants array
                //0 = 2176000 Bitrate (Highest) | mp4
                //1 = 632000 Bitrate (Higher) | mp4
                //2 = 950000 Bitrate (High) | mp4
                //3 = Bitrate N/A (m3u8) | m3u8

                if (twVideoDLQuality == TWVideoDLQuality.Highest)
                {
                    string id = media[0]["video_info"]["variants"][0]["url"].ToString();
                    var s = await client.GetStreamAsync(id);
                    var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await s.CopyToAsync(fs);
                }
                else if (twVideoDLQuality == TWVideoDLQuality.Higher)
                {
                    string id = media[0]["video_info"]["variants"][1]["url"].ToString();
                    var s = await client.GetStreamAsync(id);
                    var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await s.CopyToAsync(fs);
                }
                else if (twVideoDLQuality == TWVideoDLQuality.High)
                {
                    string id = media[0]["video_info"]["variants"][2]["url"].ToString();
                    var s = await client.GetStreamAsync(id);
                    var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await s.CopyToAsync(fs);
                }
                else if (twVideoDLQuality == TWVideoDLQuality.m3u8)
                {
                    string id = media[0]["video_info"]["variants"][3]["url"].ToString();
                    var s = await client.GetStreamAsync(id);
                    var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await s.CopyToAsync(fs);
                }
                statusBar.Text = "Downloaded the video successfully!";
                MessageBox.Show("Downloaded the video successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                statusBar.Text = e.Message;
            }
        }
        public Uri TwitterURIConstructor(string uri)
        {
            string APIFormat = @"https://api.twitter.com/1.1/statuses/show.json?id=";
            string entitiesFormat = @"&include_entities=true";
            string tweetID = uri.Substring(uri.Length - 19, 19);
            string constructedURI = APIFormat + tweetID + entitiesFormat;
            Uri resultantURI = new Uri(constructedURI);
            return resultantURI;
        }
        public async void TWImageDownloadAsync(Uri uri, string bearerToken, string filePath)
        { 
            var client = new HttpClient();
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, uri);
            request.Headers.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10136");
            request.Headers.Add(HeaderNames.Accept, "application/json");
            request.Headers.Add(HeaderNames.Authorization, bearerToken);
            var response = await client.SendAsync(request);
            var res = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject(res);

            JObject joResponse = JObject.Parse(json.ToString());
            JObject ojObject = (JObject)joResponse["extended_entities"];
            JArray media = (JArray)ojObject["media"];

            string id = media[0]["media_url_https"].ToString();
            var s = await client.GetStreamAsync(id);
            var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            await s.CopyToAsync(fs);
        }

        public async void YTVideoDownloadAsync(string dlURL, YTVideoDLType ytdlType, string captionLanguage = "en")
        {          
            var youtubeClient = new YoutubeClient();
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(dlURL);
            if(ytdlType == YTVideoDLType.AudioAndVideo)
            {
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
                        var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
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
                        var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
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
            else if(ytdlType == YTVideoDLType.AudioOnly)
            {
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
                    MessageBox.Show("Downloaded the audio successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            else if(ytdlType == YTVideoDLType.VideoOnly)
            {
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
        }
        public async void YTPlaylistDownloadAsync(string dlURL, YTVideoDLType ytdlType, string captionLanguage = "en")
        {
            var youtubeClient = new YoutubeClient();
            if(ytdlType == YTVideoDLType.AudioAndVideo)
            {
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
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
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
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
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
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
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
                    MessageBox.Show("Downloaded the playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    statusBar.Text = "Downloaded the playlist successfully!";
                }
            }
            else if(ytdlType == YTVideoDLType.AudioOnly)
            {
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
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
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
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
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
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
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
            else if(ytdlType == YTVideoDLType.VideoOnly)
            {
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
                await foreach (var videoP in youtubeClient.Playlists.GetVideosAsync(dlURL))
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
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
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
                            var trackInfo = trackManifest.TryGetByLanguage(captionLanguage);
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
        private void dnButton_Click(object sender, RoutedEventArgs e)
        {
            string dlURL = dnURI.Text;
            try
            {
                if (dnPlatform.SelectedItem == YouTube)
                {
                    if (dnType.SelectedItem == Video)
                    {
                        if (audioVideo.IsSelected == true)
                        {
                            YTVideoDownloadAsync(dlURL, YTVideoDLType.AudioAndVideo);
                        }
                        else if (audioOnly.IsSelected == true)
                        {
                            YTVideoDownloadAsync(dlURL, YTVideoDLType.AudioOnly);

                        }
                        else if (videoOnly.IsSelected == true)
                        {
                            YTVideoDownloadAsync(dlURL, YTVideoDLType.VideoOnly);
                        }
                    }
                    else if (dnType.SelectedItem == Playlist)
                    {
                        if (audioVideo.IsSelected == true)
                        {
                            YTPlaylistDownloadAsync(dlURL, YTVideoDLType.AudioAndVideo);
                        }
                        else if (audioOnly.IsSelected == true)
                        {
                            YTPlaylistDownloadAsync(dlURL, YTVideoDLType.AudioOnly);

                        }
                        else if (videoOnly.IsSelected == true)
                        {
                            YTPlaylistDownloadAsync(dlURL, YTVideoDLType.VideoOnly);
                        }
                    }
                }
                else if (dnPlatform.SelectedItem == Twitter)
                {
                    var bearerTokenTW = new Secrets().SetTwitterBearerToken();

                    if (dnType.SelectedItem == Video)
                    {

                        var dlURI = TwitterURIConstructor(dlURL);
                        var saveFileDialog = new SaveFileDialog();

                        saveFileDialog.Filter = "Video Files|*.mp4";
                        Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                        if (filepathDialog == true)
                        {
                            string filepath = saveFileDialog.FileName;
                        }
                        string filepathS = saveFileDialog.FileName;

                        string dlRes = dnRes.SelectedItem.ToString();
                        string videoQuality = dlRes.Remove(0, 38);
                        switch(videoQuality)
                        {
                            case "Bitrate: 2176000":
                                {
                                    try
                                    {
                                        statusBar.Text = "Downloading...";
                                        TWVideoDownloadAsync(dlURI, bearerTokenTW, filepathS, TWVideoDLQuality.Highest);  
                                    }
                                    catch (NullReferenceException)
                                    {
                                        MessageBox.Show("Invalid URL Entered or Video Resolution Unavailable!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        statusBar.Text = "Invalid URL Entered or Video Resolution Unavailable!";
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        statusBar.Text = ex.Message; 
                                    }
                                    break;
                                }
                            case "Bitrate: 950000":
                                {
                                    try
                                    {
                                        statusBar.Text = "Downloading...";
                                        TWVideoDownloadAsync(dlURI, bearerTokenTW, filepathS, TWVideoDLQuality.High);
                                        
                                    }                   
                                    catch (NullReferenceException)
                                    {
                                        MessageBox.Show("Invalid URL Entered or Video Resolution Unavailable!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        statusBar.Text = "Invalid URL Entered or Video Resolution Unavailable!";
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        statusBar.Text = ex.Message;
                                    }
                                    break;
                                }
                            case "Bitrate: 632000":
                                {
                                    try
                                    {
                                        statusBar.Text = "Downloading...";
                                        TWVideoDownloadAsync(dlURI, bearerTokenTW, filepathS, TWVideoDLQuality.Higher);
                                        

                                    }
                                    catch (NullReferenceException)
                                    {
                                        MessageBox.Show("Invalid URL Entered or Video Resolution Unavailable!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        statusBar.Text = "Invalid URL Entered or Video Resolution Unavailable!";
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        statusBar.Text = ex.Message;
                                    }
                                    break;
                                }
                        }          
                    }
                    else if (dnType.SelectedItem == Image)
                    {
                        try
                        {
                            var dlURI = TwitterURIConstructor(dlURL);
                            var saveFileDialog = new SaveFileDialog();

                            saveFileDialog.Filter = "Image Files|*.jpg";
                            Nullable<bool> filepathDialog = saveFileDialog.ShowDialog();
                            if (filepathDialog == true)
                            {
                                string filepath = saveFileDialog.FileName;
                            }
                            string filepathS = saveFileDialog.FileName;
                            statusBar.Text = "Downloading...";
                            TWImageDownloadAsync(dlURI, bearerTokenTW, filepathS);
                            MessageBox.Show("Downloaded the image successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            statusBar.Text = "Downloaded the image successfully!";
                        }
                        catch(NullReferenceException)
                        {
                            MessageBox.Show("Invalid URL Entered!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            statusBar.Text = "Invalid URL Entered!";
                        }
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
            if(dnPlatform.SelectedItem == Twitter)
            {
                if(dnType.SelectedItem == Image)
                {
                    if (dnURI.Text != "")
                    {
                        placeholder.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        placeholder.Visibility = Visibility.Visible;
                    }
                    dnButton.Visibility = Visibility.Visible;
                }
                else if(dnType.SelectedItem == Video)
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

            }
            else if(dnPlatform.SelectedItem == YouTube)
            {
                if(dnType.SelectedItem == Video)
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
                else if(dnType.SelectedItem == Playlist)
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
        }
        private void dnOpts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dnPlatform.SelectedItem == YouTube)
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
            else if(dnPlatform.SelectedItem == Twitter)
            {
                if(dnType.SelectedItem == Video)
                {
                    dnButton.Content = "Download";
                    dnButton.Visibility = Visibility.Visible;
                }
            }
                
        }

        private void dnPlatform_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dnPlatform.SelectedItem == YouTube)
            {
                dnType.Items.Remove(Image);
                if(dnType.Items.Contains(Playlist) == false)
                {
                    dnType.Items.Add(Playlist);
                }
                dnTypeLabel.Visibility = Visibility.Visible;
                dnType.Visibility = Visibility.Visible;       
                
            }
            else if(dnPlatform.SelectedItem == Twitter)
            {
                dnType.Items.Remove(Playlist);
                if(dnType.Items.Contains(Image) == false)
                {
                    dnType.Items.Add(Image);
                }
                dnTypeLabel.Visibility = Visibility.Visible;
                dnType.Visibility = Visibility.Visible;
            }
            
        }

        private void dnRes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dnPlatform.SelectedItem == YouTube)
            {
                if(dnType.SelectedItem == Video)
                {
                    dnOpts.Visibility = Visibility.Visible;
                    dnOptsLabel.Visibility = Visibility.Visible;
                }
                else if(dnType.SelectedItem == Playlist)
                {
                    dnOpts.Visibility = Visibility.Visible;
                    dnOptsLabel.Visibility = Visibility.Visible;
                }
            }
            else if(dnPlatform.SelectedItem == Twitter)
            {
                if(dnType.SelectedItem == Video)
                {
                    dnButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void dnType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dnPlatform.SelectedItem == YouTube)
            {
                if(dnType.SelectedItem == Video)
                {
                    dnURILabel.Content = "Video URL:";
                    placeholder.Content = "Enter the video URL...";
                    dnURILabel.Visibility = Visibility.Visible;
                    dnURI.Visibility = Visibility.Visible;
                    placeholder.Visibility = Visibility.Visible;
                    dnURI.Text = "";
                    
                    //Resolution Items Logic {Start}
                    dnRes.Items.Remove(bit2176000);
                    dnRes.Items.Remove(bit632000);
                    dnRes.Items.Remove(bit950000);
                    if (dnRes.Items.Contains(MaxQuality) == false)
                    {
                        dnRes.Items.Add(MaxQuality);
                    }
                    if (dnRes.Items.Contains(p144) == false)
                    {
                        dnRes.Items.Add(p144);
                    }
                    if (dnRes.Items.Contains(p240) == false)
                    {
                        dnRes.Items.Add(p240);
                    }
                    if (dnRes.Items.Contains(p360) == false)
                    {
                        dnRes.Items.Add(p360);
                    }
                    if (dnRes.Items.Contains(p480) == false)
                    {
                        dnRes.Items.Add(p480);
                    }
                    if (dnRes.Items.Contains(p720) == false)
                    {
                        dnRes.Items.Add(p720);
                    }
                    if (dnRes.Items.Contains(p1080) == false)
                    {
                        dnRes.Items.Add(p1080);
                    }
                    if (dnRes.Items.Contains(p1440) == false)
                    {
                        dnRes.Items.Add(p1440);
                    }
                    if (dnRes.Items.Contains(p2160) == false)
                    {
                        dnRes.Items.Add(p2160);
                    }
                    //Resolution Items Logic {End}
                }
                else if(dnType.SelectedItem == Playlist)
                {
                    dnURILabel.Content = "Playlist URL:";
                    placeholder.Content = "Enter the playlist URL...";
                    dnURILabel.Visibility = Visibility.Visible;
                    dnURI.Visibility = Visibility.Visible;
                    dnURI.Text = "";
                    placeholder.Visibility = Visibility.Visible;

                    //Resolution Items Logic {Start}
                    dnRes.Items.Remove(bit2176000);
                    dnRes.Items.Remove(bit632000);
                    dnRes.Items.Remove(bit950000);
                    if (dnRes.Items.Contains(MaxQuality) == false)
                    {
                        dnRes.Items.Add(MaxQuality);
                    }
                    if (dnRes.Items.Contains(p144) == false)
                    {
                        dnRes.Items.Add(p144);
                    }
                    if (dnRes.Items.Contains(p240) == false)
                    {
                        dnRes.Items.Add(p240);
                    }
                    if (dnRes.Items.Contains(p360) == false)
                    {
                        dnRes.Items.Add(p360);
                    }
                    if (dnRes.Items.Contains(p480) == false)
                    {
                        dnRes.Items.Add(p480);
                    }
                    if (dnRes.Items.Contains(p720) == false)
                    {
                        dnRes.Items.Add(p720);
                    }
                    if (dnRes.Items.Contains(p1080) == false)
                    {
                        dnRes.Items.Add(p1080);
                    }
                    if (dnRes.Items.Contains(p1440) == false)
                    {
                        dnRes.Items.Add(p1440);
                    }
                    if (dnRes.Items.Contains(p2160) == false)
                    {
                        dnRes.Items.Add(p2160);
                    }
                    //Resolution Items Logic {End}
                }
            }
            if(dnPlatform.SelectedItem == Twitter)
            {
                if(dnType.SelectedItem == Image)
                {
                    dnURILabel.Visibility = Visibility.Visible;
                    dnURILabel.Content = "Image URL: ";
                    dnURI.Visibility = Visibility.Visible;
                    placeholder.Content = "Enter the image URL...";
                    placeholder.Visibility = Visibility.Visible;
                    if (dnResLabel.Visibility == Visibility.Visible)
                    {
                        dnResLabel.Visibility = Visibility.Hidden;
                        dnRes.Visibility = Visibility.Hidden;
                    }
                    if (dnOptsLabel.Visibility == Visibility.Visible)
                    {
                        dnOptsLabel.Visibility = Visibility.Hidden;
                        dnOpts.Visibility = Visibility.Hidden;
                    }
                    if (dnCC.Visibility == Visibility.Visible)
                    {
                        dnCC.Visibility = Visibility.Hidden;
                        dnButton.Visibility = Visibility.Hidden;
                    }
                }
                else if(dnType.SelectedItem == Video)
                {
                    dnURILabel.Content = "Video URL:";
                    placeholder.Content = "Enter the video URL...";
                    dnURILabel.Visibility = Visibility.Visible;
                    dnURI.Visibility = Visibility.Visible;
                    placeholder.Visibility = Visibility.Visible;
                    dnURI.Text = "";

                    //Resolution Items Logic {Start}
                    dnRes.Items.Remove(p144);
                    dnRes.Items.Remove(p240);
                    dnRes.Items.Remove(p360);
                    dnRes.Items.Remove(p480);
                    dnRes.Items.Remove(p720);
                    dnRes.Items.Remove(p1080);
                    dnRes.Items.Remove(p1440);
                    dnRes.Items.Remove(p2160);
                    dnRes.Items.Remove(MaxQuality);
                    if (dnRes.Items.Contains(bit2176000) == false)
                    {
                        dnRes.Items.Add(bit2176000);
                    }
                    if (dnRes.Items.Contains(bit632000) == false)
                    {
                        dnRes.Items.Add(bit632000);
                    }
                    if (dnRes.Items.Contains(bit950000) == false)
                    {
                        dnRes.Items.Add(bit950000);
                    }
                    //Resolution Items Logic {End}
                }
            }
        }
    }
}
