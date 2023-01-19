using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RadarGame.UI.Windows;
using RadarGame.UI.ViewModels;

namespace RadarGame.UI.Controls
{
    /// <summary>
    /// Interaction logic for WindowControl.xaml
    /// </summary>
    public partial class WindowControl : UserControl
    {
        
        public bool CloseItem
        {
            get { return (bool)GetValue(CloseItemProperty); }
            set { SetValue(CloseItemProperty, value); }
        }

        public static readonly DependencyProperty CloseItemProperty =
            DependencyProperty.Register("CloseItem", typeof(bool), typeof(WindowControl),
                new PropertyMetadata(false, WindowControlChangedCallback));

        private static void WindowControlChangedCallback(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WindowControl control = (WindowControl)d;
            control.CloseItem = (bool)e.NewValue;
            if (control.CloseItem)
            {
                control.CloseButton.Visibility = Visibility.Visible;
                control.MinimizeToTrayButton.Visibility = Visibility.Hidden;
                control.MinimizeButton.Visibility = Visibility.Hidden;
            }
        }

        public WindowControl()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = RadarGameMessageBox.Show("Radar Service will be turned off.\nAre you willing to exit?",
                "Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Window parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
        }
        private void MinimizeToTrayButton_MinimizeToTray(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if ((parentWindow.Content as Grid).DataContext is MainViewModel mainViewModel && mainViewModel.ProtocolsViewModel.IsTurnedOn)
            {
                parentWindow.Hide();
            }
            else
            {
                parentWindow?.Close();
            }
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
                    Content = String.Format("Close")
                };
                (sender as Button).ToolTip = maximize;
            }
        }
    }
}
