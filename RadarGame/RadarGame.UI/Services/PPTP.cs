using DeviceId;
using DotRas;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RadarGame.UI.Interfaces;
using RadarGame.UI.Tools;

namespace RadarGame.UI.Services
{
    public class PPTP : IService
    {
        //Fields
        private IConnectionObserver connectionObserver;
        private VpnNetworkManager networkManager;
        private RasDialer dialer = new RasDialer();
        private RasHandle handle;
        public string ServiceText { get => "...درحال اتصال"; }

        //Constructor
        public PPTP(IConnectionObserver connectionObserver,
            VpnNetworkManager networkManager)
        {
            this.connectionObserver = connectionObserver;
            this.networkManager = networkManager;
        }

        #region Private Methods
        private async void Dialer_DialCompleted(object sender, DialCompletedEventArgs e)
        {
            if (!connectionObserver.IsGettingData)
            {
                Dispose();
                return;
            }

            if (e.TimedOut || e.Error != null)
            {
                connectionObserver.ConnectionObserver(false, "خطا");
                RadarLogger.GetInstance().Logger.Error(e.Error);
            }

            //"The VPN connection between your computer and the VPN server could not be completed. The most common cause for this failure is that at least one Internet device (for example, a firewall or a router) between your computer and the VPN server is not configured to allow Generic Routing Encapsulation (GRE) protocol packets. If the problem persists, contact your network administrator or Internet Service Provider."

            else if (e.Connected)
            {
                await Task.Factory.StartNew(async () =>
                {
                    string deviceId = new DeviceIdBuilder().AddMacAddress().AddUserName().ToString();
                    await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.HardwareID + deviceId);
                    networkManager.StartVpnConnectionObserver();
                    connectionObserver.ConnectionObserver(true, "وصل");
                });
                
            }
        }

        private async Task StopProcess()
        {
            if (dialer.IsBusy)
            {
                dialer.DialAsyncCancel();
            }
            else
            {
                if (handle != null)
                {
                    RasConnection Connection = RasConnection.GetActiveConnectionByHandle(handle);
                    if (Connection != null)
                    {
                        Connection.HangUp();
                    }
                }
            }

            using (RasPhoneBook PhoneBook = new RasPhoneBook())
            {
                PhoneBook.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers));
                
                if (PhoneBook.Entries.Contains(MyConnection._ConnectionName))
                {
                    PhoneBook.Entries.Remove(MyConnection._ConnectionName);
                }
            }

            var tempDirectory = DirectoryHelperFunctions.GetTemporaryDirectory();
            string pathToRouteBatch = tempDirectory + "//routedel.bat";
            var routeBatch = await RadarHttpClient.GetInstance().Client.GetStringAsync("http://cdn.radar.game/app/vpn/routes/routedel.bat");
            await DirectoryHelperFunctions.WriteAsync(routeBatch, pathToRouteBatch);
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.FileName = @"cmd.exe";
            psi.Verb = "runas";

            psi.Arguments = "/C \"" + pathToRouteBatch;

            Process proc = new Process();
            proc.StartInfo = psi;
            proc.Start();
        }

        private string findMask(string CID)
        {
            int sMask = Convert.ToInt32(CID);
            int index = sMask / 8;
            int multiply = sMask % 8;
            int lastIndex = 0;
            int startPoint = 128;
            List<int> indexes = new List<int>();
            for (int i = 0; i < multiply; i++)
            {
                if (i != 0)
                {
                    startPoint /= 2;
                }
                lastIndex += startPoint;
            }

            for (int i = 0; i < index; i++)
            {
                indexes.Add(255);
            }

            if (indexes.Count < 4)
            {
                indexes.Add(lastIndex);
                while (indexes.Count < 4)
                {
                    indexes.Add(0);
                }
            }

            return string.Join(".", indexes);
        }
        #endregion

        #region Public Methods
        public async Task<bool> Connect()
        {
            try
            {
                //make sure that there is no connection enabled.
                await StopProcess();
                var ServerIp = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.PptpIp);
                using (RasPhoneBook PhoneBook = new RasPhoneBook())
                {
                    PhoneBook.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers));
                    RasEntry Entry;

                    if (PhoneBook.Entries.Contains(MyConnection._ConnectionName))
                    {
                        PhoneBook.Entries.Remove(MyConnection._ConnectionName);
                    }

                    Entry = RasEntry.CreateVpnEntry(MyConnection._ConnectionName, 
                        ServerIp, 
                        RasVpnStrategy.PptpOnly, 
                        RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn));

                    Entry.Options.PreviewDomain = false;
                    Entry.Options.ShowDialingProgress = false;
                    Entry.Options.PromoteAlternates = false;
                    Entry.Options.DoNotNegotiateMultilink = false;
                    Entry.Options.RequirePap = true;
                    Entry.Options.RequireChap = true;
                    Entry.Options.RequireMSChap2 = true;
                    Entry.Options.RequireEncryptedPassword = false;
                    Entry.Options.RequireDataEncryption = false;
                    Entry.EncryptionType = RasEncryptionType.None;
                    Entry.IPv4InterfaceMetric = 1;
                    Entry.DnsAddress = IPAddress.Parse("10.8.0.1");
                    Entry.Options.RemoteDefaultGateway = false;
                    PhoneBook.Entries.Add(Entry);

                    dialer.EntryName = MyConnection._ConnectionName;
                    dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
                    dialer.Credentials = new NetworkCredential(MyConnection._username, MyConnection._password);
                    dialer.DialCompleted -= Dialer_DialCompleted;
                    dialer.DialCompleted += Dialer_DialCompleted;
                    handle = dialer.DialAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                RadarLogger.GetInstance().Logger.Error(e);
                return false;
            }
        }

        public async Task Dispose()
        {
            connectionObserver?.ConnectionObserver(null, "...درحال قطع اتصال");
            await StopProcess();
            connectionObserver?.ConnectionObserver(false, "قطع");
        }
        #endregion
    }
}
