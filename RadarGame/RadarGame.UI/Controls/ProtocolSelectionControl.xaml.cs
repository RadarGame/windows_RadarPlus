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
    /// Interaction logic for ProtocolSelectionControl.xaml
    /// </summary>
    public partial class ProtocolSelectionControl : UserControl
    {
        public static readonly DependencyProperty PPTPCommandProperty = DependencyProperty.Register(
            "PPTPCommand", typeof(ICommand), typeof(ProtocolSelectionControl), new PropertyMetadata(default(ICommand)));

        public ICommand PPTPCommand
        {
            get { return (ICommand)GetValue(PPTPCommandProperty); }
            set { SetValue(PPTPCommandProperty, value); }
        }

        public static readonly DependencyProperty OpenVPNCommandProperty = DependencyProperty.Register(
            "OpenVPNCommand", typeof(ICommand), typeof(ProtocolSelectionControl), new PropertyMetadata(default(ICommand)));

        public ICommand OpenVPNCommand
        {
            get { return (ICommand)GetValue(OpenVPNCommandProperty); }
            set { SetValue(OpenVPNCommandProperty, value); }
        }

        public static readonly DependencyProperty SoftEtherCommandProperty = DependencyProperty.Register(
            "SoftEtherCommand", typeof(ICommand), typeof(ProtocolSelectionControl), new PropertyMetadata(default(ICommand)));

        public ICommand SoftEtherCommand
        {
            get { return (ICommand)GetValue(SoftEtherCommandProperty); }
            set { SetValue(SoftEtherCommandProperty, value); }
        }

        public ProtocolSelectionControl()
        {
            InitializeComponent();
        }
    }
}
