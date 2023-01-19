using DotRas;
using RadarGame.UI.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using RadarGame.UI.Interfaces;
using RadarGame.UI.Tools;

namespace RadarGame.UI.Services
{
    public class DNSService : IDNSService
    {
        //Fields
        private static readonly string PrimaryDNS = "";
        private static readonly string SecondaryDNS = "";
        private static string[] dns = { PrimaryDNS, SecondaryDNS };
        private static string[] currentDns;

        #region Properties(Getter, Setter)

        #endregion

        //Constructor
        public DNSService() => Load();
        #region Private Methods
        private async void Load()
        {
            try
            {
                await CheckDNS();
            }
            catch (Exception e)
            {
                RadarLogger.GetInstance().Logger.Error(e);
            }
        }
        private async Task CheckDNS()
        {
            var data = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.SettingsJson);
            if (data == null)
            {
                data = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.SettingsJson);
            }
            var objects = JsonConvert.DeserializeObject<Rootobject>(data);
            var dnsAddress = await GetDnsAddress();

            if (dnsAddress != null)
            {
                List<string> list = new List<string>{objects.dns1, objects.dns2};
                foreach (string dns in list)
                {
                    if (dnsAddress.Any(s => s == dns))
                    {
                        return;
                    }
                }
                currentDns = new string[dnsAddress.Count];
                for (int i = 0; i < dnsAddress.Count; i++)
                {
                    currentDns[i] = dnsAddress[i];
                }
            }
        }
        private async Task<List<string>> GetDnsAddress()
        {
            NetworkInterface networkInterface = await GetActiveEthernetOrWifiNetworkInterface();
            List<string> addresses = new List<string>();
            if (networkInterface == null)
            {
                RadarGameMessageBox.Show("وی پی ان خود را خاموش کنید");
            }
            else
            {
                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;
                IPAddressCollection dhcpAddressCollection = ipProperties.DhcpServerAddresses;

                foreach (var dns in dnsAddresses)
                {
                    if (dhcpAddressCollection.Any(s => Equals(s, dns)))
                    {
                        return null;
                    }
                }
                foreach (IPAddress dnsAddress in dnsAddresses)
                {
                    addresses.Add(dnsAddress.ToString());
                }
            }
            return addresses;
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
        private async Task SetDNS(string[] Dns)
        {
            NetworkInterface networkInterface = await GetActiveEthernetOrWifiNetworkInterface();
            if (networkInterface == null)
                return;
            foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
                {
                    ManagementBaseObject methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
                    if (methodParameters != null)
                    {
                        methodParameters["DNSServerSearchOrder"] = (object)Dns;
                        instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
                    }
                }
            }
        }
        #endregion

        #region public Methods
        public async Task GetDataFromServerAndSetDNS()
        {
            var data = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.SettingsJson);
            if (data == null)
            {
                data = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.SettingsJson);
            }
            var objects = JsonConvert.DeserializeObject<Rootobject>(data);
            dns = new[] { objects?.dns1, objects?.dns2 };
            await SetDNS(dns);
        }

        public async Task UnsetDNS()
        {
            NetworkInterface networkInterface = await GetActiveEthernetOrWifiNetworkInterface();
            if (networkInterface == null)
                return;
            foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
                {
                    ManagementBaseObject methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
                    if (methodParameters != null)
                    {
                        methodParameters["DNSServerSearchOrder"] = currentDns;
                        instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
                    }
                }
            }
        }

        public static void UnsetDnsEvent()
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                     (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                     a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
            if (networkInterface == null)
                return;
            foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
                {
                    ManagementBaseObject methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
                    if (methodParameters != null)
                    {
                        methodParameters["DNSServerSearchOrder"] = currentDns;
                        instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
                    }
                }
            }

            var openVpnProcess = Process.GetProcesses().
                Where(pr => pr.ProcessName == "openvpn");

            foreach (var process in openVpnProcess)
                process.Kill();
        }

        public async Task<bool> Connect()
        {
            try
            {
                await GetDataFromServerAndSetDNS();

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
            await UnsetDNS();
        }
        #endregion
    }
}
