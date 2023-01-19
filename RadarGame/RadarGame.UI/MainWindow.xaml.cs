using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
using RadarGame.UI.Windows;
using RadarGame.UI.Tools;

namespace RadarGame.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        #region Private Methods

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }
        #region Public Methods

        #endregion
      
        private async Task GetDnsAddress()
        {
            NetworkInterface networkInterface = await GetActiveEthernetOrWifiNetworkInterface();
            List<string> addresses = new List<string>();
            if (networkInterface == null)
            {
                RadarGameMessageBox.Show("وی پی ان خود را خاموش کنید");
            }
        }
        private static async Task<NetworkInterface> GetActiveEthernetOrWifiNetworkInterface()
        {
            return await Task.Run(() =>
            {
                var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                    a => a.OperationalStatus == OperationalStatus.Up &&
                         (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                         a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
                return Nic;
            });
        }
        #endregion
    }
}

