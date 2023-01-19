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
    /// Interaction logic for MessageBoxDialogHeaderControl.xaml
    /// </summary>
    public partial class MessageBoxDialogHeaderControl : UserControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MessageBoxDialogHeaderControl),
                new PropertyMetadata(string.Empty));


        public bool CanResize
        {
            get { return (bool)GetValue(CanResizeProperty); }
            set { SetValue(CanResizeProperty, value); }
        }
        public static readonly DependencyProperty CanResizeProperty =
            DependencyProperty.Register("CanResize", typeof(bool), typeof(MessageBoxDialogHeaderControl),
                new PropertyMetadata(true, canResizeChanged));

        private static void canResizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBoxDialogHeaderControl instance = d as MessageBoxDialogHeaderControl;
            instance.ApplyTemplate();
            MessageBoxWindowControl winControl = (MessageBoxWindowControl)instance.Template.FindName("messageBoxWindowControl", instance);
            winControl.CloseItem = !(bool)e.NewValue;
        }
        public MessageBoxDialogHeaderControl()
        {
            InitializeComponent();
        }
    }
}
