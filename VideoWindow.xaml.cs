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
using System.Windows.Shapes;
using System.Windows.Threading;
using YoutubeExplode;

namespace FilumDLWPF
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// FilumPlayer: v0.0.5
    /// Copyright: Blaze Devs 2022-2024, All Rights Reserved
    /// Warning: The video player is not fully functional and is still in development, there might be some bugs and errors in the video player.
    /// </summary>
    public partial class VideoWindow : Window
    {
        public VideoWindow()
        {
            InitializeComponent();
        }

        public async Task VideoPlayer(string videoId)
        {
            
            try
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(videoId);
                int duration = (int)video.Duration.Value.TotalSeconds;
                finalTime.Content = TimeSpan.FromSeconds(duration).ToString(@"hh\:mm\:ss"); //Display the final time
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
                var streamInfo = streamManifest.GetMuxedStreams().FirstOrDefault(s => s.VideoQuality.Label == "720p");
                if (streamInfo != null)
                {
                    mediaPlayer.Source = new Uri(streamInfo.Url);
                }
                //Thumbnail
                thumbnail.Source = new BitmapImage(new Uri(video.Thumbnails.First().Url));

                
                //Play the video
                playButton.Click += (sender, e) => mediaPlayer.Play();
                playButton.Click += (sender, e) =>
                {
                    greenScreen.Visibility = Visibility.Collapsed;
                    thumbnail.Visibility = Visibility.Collapsed;
                    mediaPlayer.Visibility = Visibility.Visible;
                    mediaPlayer.Play();
                    // Hide the thumbnail when the video is played
                    pauseButton.IsEnabled = true;
                    playButton.IsEnabled = false;
                };
                //Pause the video
                pauseButton.Click += (sender, e) =>
                {
                    mediaPlayer.Pause();
                    pauseButton.IsEnabled = false;
                    playButton.IsEnabled = true;
                };

                //Volume control
                volumeSlider.ValueChanged += (sender, e) => mediaPlayer.Volume = volumeSlider.Value;

                //Stop the video
                stopButton.Click += (sender, e) =>
                {
                    greenScreen.Visibility = Visibility.Collapsed;
                    thumbnail.Visibility = Visibility.Visible;
                    mediaPlayer.Visibility = Visibility.Collapsed;
                    mediaPlayer.Stop();
                };

                //Timeline slider control
                mediaPlayer.MediaOpened += (sender, e) =>
                {
                    var timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Tick += (s, e) =>
                    {
                        timeSlider.Value = mediaPlayer.Position.TotalSeconds;
                        currentTime.Content = mediaPlayer.Position.ToString(@"hh\:mm\:ss");

                    };
                    timeSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    timeSlider.Value = 0;
                    timeSlider.SmallChange = 1;
                    timeSlider.LargeChange = Math.Min(10, mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds / 10);
                    timer.Start();
                };
                timeSlider.Minimum = 0;
                timeSlider.Maximum = duration;

                timeSlider.MouseLeftButtonUp += (s, e) => mediaPlayer.Position = TimeSpan.FromSeconds(timeSlider.Value);
                timeSlider.ValueChanged += (s, e) =>
                {
                    mediaPlayer.Position = TimeSpan.FromSeconds(timeSlider.Value);
                };


                mediaPlayer.MediaEnded += (sender, e) =>
                {
                    playButton.IsEnabled = true;
                    pauseButton.IsEnabled = false;
                    mediaPlayer.Stop();
                };
                //Thumbnail and Media Player Visibility
                if (thumbnail.Visibility == Visibility.Collapsed)
                {
                    mediaPlayer.Visibility = Visibility.Visible;
                }
                else if (thumbnail.Visibility == Visibility.Visible)
                {
                    mediaPlayer.Visibility = Visibility.Collapsed;
                }
                this.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to play the video, please try again later!", "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}
