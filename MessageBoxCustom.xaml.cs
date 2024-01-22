using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace FilumDLWPF
{
    ///<summary>
    ///Custom MessageBox for WPF applications developed by Blaze Devs & Shubhojeet Choudhury: FilumMsgBox v0.0.5
    /// Copyright: Blaze Devs 2022-2024, All Rights Reserved
    ///This is a custom MessageBox that can be used in any WPF application
    ///Warning: This is not a fully functional MessageBox and is still in development, there might be some bugs and errors in the MessageBox.
    ///</summary>
    public partial class MessageBoxCustom : Window
    {
        public enum MessageBoxType
        {
            Error = 0,
            Information = 1,
            Warning = 2,
            Question = 3,
            Success = 4
        }
        public enum MessageBoxButtonTypes
        {
            Ok = 0,
            OkCancel = 1,
            YesNo = 2,
            YesNoCancel = 3
        }
        public Image ByteArrayToImage(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = ms;
            imageSource.EndInit();
            return new Image { Source = imageSource };
        }

        public MessageBoxCustom(string message = "", string title = "", MessageBoxType messageBoxType = MessageBoxType.Information, MessageBoxButtonTypes messageBoxButtonTypes = MessageBoxButtonTypes.Ok,  ImageSource icon = null, Brush background = null, Brush foreground = null, FontFamily font = null, double fontSize = 16)
        {
            
            InitializeComponent();
            Title = title;
            MessageTextBlock.FontWeight = FontWeights.Bold;
            MessageTextBlock.Text = message;
            MessageTextBlock.Background = background ?? Brushes.White;
            MessageTextBlock.Foreground = foreground ?? Brushes.Black;
            MessageTextBlock.FontFamily = font ?? new FontFamily("Arial");
            MessageTextBlock.FontSize = fontSize;
            if (icon != null)
            {
                Icon = icon;
            }
            else
            {
                var errorIcon = ByteArrayToImage(Properties.Resources.error);
                var infoIcon = ByteArrayToImage(Properties.Resources.information);
                var warningIcon = ByteArrayToImage(Properties.Resources.warning);
                var questionIcon = ByteArrayToImage(Properties.Resources.help);
                var successIcon = ByteArrayToImage(Properties.Resources.success);
                switch (messageBoxType)
                {
                    case MessageBoxType.Error:
                        Icon = errorIcon.Source;
                        MsgIcon.Source = errorIcon.Source;
                        break;
                    case MessageBoxType.Information:
                        Icon = infoIcon.Source;
                        MsgIcon.Source = infoIcon.Source;
                        break;
                    case MessageBoxType.Warning:
                        Icon = warningIcon.Source;
                        MsgIcon.Source = warningIcon.Source;
                        break;
                    case MessageBoxType.Question:
                        Icon = questionIcon.Source;
                        MsgIcon.Source = questionIcon.Source;
                        break;
                    case MessageBoxType.Success:
                        Icon = successIcon.Source;
                        MsgIcon.Source = successIcon.Source;
                        break;
                    default:
                        Icon = infoIcon.Source;
                        MsgIcon.Source = infoIcon.Source;
                        break;
                }
                
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        public static bool? ShowDialogBox(string message, string title, MessageBoxType messageBoxType = MessageBoxType.Information, MessageBoxButtonTypes messageBoxButtonTypes = MessageBoxButtonTypes.Ok, Brush background = null, Brush foreground = null, FontFamily font = null, double fontSize = 16, ImageSource icon = null)
        {
            MessageBoxCustom messageBox = new MessageBoxCustom(message, title,messageBoxType,messageBoxButtonTypes, icon, background, foreground, font, fontSize);
            return messageBox.ShowDialog();
        }
        private void ExtraButton_Click(object sender, RoutedEventArgs e)
        {
            // Add your code here for the extra button
        }

    }
}
