using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        private void YT_Selected(object sender, RoutedEventArgs e)
        {
            exBtn.Visibility = Visibility.Visible;
        }
        private void exBtn_Click(object sender, RoutedEventArgs e)
        {
            if(exType.SelectedItem == YouTube)
            {
                YTWindow yTWindow = new YTWindow();
                yTWindow.Show();
                this.Close();
            }

            else if(exType.SelectedItem == Spotify)
            {
                SpotWindow spotWindow = new SpotWindow();
                spotWindow.Show();
                this.Close();
            }
        }


        private void Spotify_Selected(object sender, RoutedEventArgs e)
        {
            exBtn.Visibility = Visibility.Visible;
        }
    }
}
