using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace RadarGame.UI.Windows
{
    /// <summary>
    /// Interaction logic for RadarGameMessageBox.xaml
    /// </summary>
    public partial class RadarGameMessageBox : Window
    {
        private MessageBoxResult result;

        public RadarGameMessageBox(string messageBoxText, string caption, MessageBoxButton buttons)
        {
            InitializeComponent();

            MainTextBlock.Text = messageBoxText;
            MessageHeader.Title = caption;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        public static MessageBoxResult Show(string messageBoxText, [Optional] string caption, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return Application.Current.Dispatcher.Invoke(new Func<MessageBoxResult>(() =>
            {
                if (caption == null)
                    caption = "هشدار";

                RadarGameMessageBox messageBox =
                    new RadarGameMessageBox(messageBoxText, caption, buttons);
                if (Application.Current.MainWindow.IsLoaded)
                {
                    messageBox.Owner = Application.Current.MainWindow;
                }
                else
                {
                    messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                return messageBox.Show();
            }));
        }

        public new MessageBoxResult Show()
        {
            ShowDialog();
            return result;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            Close();
        }
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            Close();
        }
        private void No_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            Close();
        }
    }
}
