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

namespace RadarGame.UI.Controls
{
    /// <summary>
    /// Interaction logic for MessageBoxWindowControl.xaml
    /// </summary>
    public partial class MessageBoxWindowControl : UserControl
    {
        public bool CloseItem
        {
            get { return (bool)GetValue(CloseItemProperty); }
            set { SetValue(CloseItemProperty, value); }
        }

        public static readonly DependencyProperty CloseItemProperty =
            DependencyProperty.Register("CloseItem", typeof(bool), typeof(MessageBoxWindowControl),
                new PropertyMetadata(false, WindowControlChangedCallback));

        private static void WindowControlChangedCallback(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBoxWindowControl control = (MessageBoxWindowControl)d;
            control.CloseItem = (bool)e.NewValue;
            if (control.CloseItem)
            {
                control.CloseButton.Visibility = Visibility.Visible;
                control.MinimizeToTrayButton.Visibility = Visibility.Hidden;
                control.MinimizeButton.Visibility = Visibility.Hidden;
            }
        }

        public MessageBoxWindowControl()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
        private void MinimizeToTrayButton_MinimizeToTray(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Hide();
        }
        private void MinimizeButton_Minimize(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;
        }

        private void MinimizeToTrayButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow.WindowState == WindowState.Normal)
            {
                ToolTip maximize = new ToolTip
                {
                    Content = String.Format("Minimize To Tray")
                };
                (sender as Button).ToolTip = maximize;
            }
        }
    }
}
