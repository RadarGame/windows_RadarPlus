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
            //_ = new AuthenticationViewModel();
            //try
            //{
            //    string path = AppContext.BaseDirectory + @"openVPN\Batch.txt";
            //    if (!File.Exists(path))
            //    {
            //        InstallTapAdapter();
            //        File.Create(path);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    RadarLogger.GetInstance().Logger.Error(ex);
            //}
            //Loaded += MainWindow_Loaded;
        }
        #region Private Methods

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }
        #region Public Methods

        #endregion
        public bool InstallTapAdapter()
        {
            bool installed = false;
            ProcessStartInfo processInfo = null;
            Process proc = new System.Diagnostics.Process();
            try
            {
                /*
                proc.StartInfo.FileName = AppContext.BaseDirectory+"\\openVPN\\Driver\\addtap.bat";
                proc.StartInfo.Verb = "runas";
                proc.StartInfo.WorkingDirectory = AppContext.BaseDirectory + "\\openVPN\\Driver";
                proc.Start();
                */
                string str = proc.StandardOutput.ReadToEnd();
                string err = proc.StandardError.ReadToEnd();
                int exitCode = proc.ExitCode;

                if (err.Length > 0)
                    throw new Exception(err);

                // Write into logs
                RadarLogger.GetInstance().Logger.Info("COMPLETED Installing tap Exit code = " + exitCode);

                if (str.IndexOf("Drivers installed successfully") > -1)

                {
                    installed = true;
                    // Write into logs  
                    RadarLogger.GetInstance().Logger.Info("Tap Adapter Installed Successfully");
                }
                // Write into logs
                RadarLogger.GetInstance().Logger.Info("Finished TAP");
            }
            catch (Exception e)
            {
                // Write into logs
                RadarLogger.GetInstance().Logger.Error("Error Installing Tap Adapter : " + e.Message);
            }
            finally
            {
                processInfo = null;
                if (proc != null)
                {
                    proc.Close();
                    proc = null;
                }
            }
            return installed;
        }
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

