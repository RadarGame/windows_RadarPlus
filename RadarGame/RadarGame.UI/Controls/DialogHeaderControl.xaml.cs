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
    /// Interaction logic for DialogHeaderControl.xaml
    /// </summary>
    public partial class DialogHeaderControl : UserControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DialogHeaderControl),
                new PropertyMetadata(string.Empty));


        public bool CanResize
        {
            get { return (bool)GetValue(CanResizeProperty); }
            set { SetValue(CanResizeProperty, value); }
        }
        public static readonly DependencyProperty CanResizeProperty =
            DependencyProperty.Register("CanResize", typeof(bool), typeof(DialogHeaderControl),
                new PropertyMetadata(true, canResizeChanged));

        private static void canResizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogHeaderControl instance = d as DialogHeaderControl;
            instance.ApplyTemplate();
            WindowControl winControl = (WindowControl)instance.Template.FindName("windowControl", instance);
            winControl.CloseItem = !(bool)e.NewValue;
        }

        public DialogHeaderControl()
        {
            InitializeComponent();
        }
    }
}
