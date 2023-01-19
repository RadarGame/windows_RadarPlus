using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RadarGame.UI.Models;
using RadarGame.UI.Tools;

namespace RadarGame.UI.Services
{
    public class VpnNetworkManager
    {
        private Timer timer;
        private IConnectionObserver connectionObserver;

        public VpnNetworkManager(IConnectionObserver connectionObserver)
        {
            this.connectionObserver = connectionObserver;
        }
        public void StartVpnConnectionObserver()
        {
            timer = new Timer(checkConnection, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public void StopVpnConnectionObserver()
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
            if (timer != null)
            {
                timer.Dispose();
            }
        }

        public async Task<bool> ChangeInterfaceMetric(bool vpnConnected)
        {
            if (vpnConnected)
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        FileName = @"netsh.exe",
                        Arguments = $"interface ipv4 set interface \"{GetVpnNetworkInterface().Name}\" metric=1",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                bool started = p.Start();

                if (started)
                {
                    if (SpinWait.SpinUntil(() => p.HasExited, TimeSpan.FromSeconds(20)))
                    {
                        await Task.Delay(500);
                        System.Diagnostics.Process p2 = new System.Diagnostics.Process
                        {
                            StartInfo =
                            {
                                FileName = @"netsh.exe",
                                Arguments = $"interface ipv4 set interface \"{GetInternetNetworkInterface().Name}\" metric=100",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true
                            }
                        };

                        bool started2 = p2.Start();
                        if (started2)
                        {
                            if (SpinWait.SpinUntil(() => p2.HasExited, TimeSpan.FromSeconds(20)))
                            {
                                await Task.Delay(500);
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Process p2 = new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        FileName = @"netsh.exe",
                        Arguments = $"interface ipv4 set interface \"{GetInternetNetworkInterface().Name}\" metric=0",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                bool started2 = p2.Start();
                if (started2)
                {
                    if (SpinWait.SpinUntil(() => p2.HasExited, TimeSpan.FromSeconds(20)))
                    {
                        await Task.Delay(500);
                        return true;
                    }
                }
            }


            return false;
        }

        public NetworkInterface GetInternetNetworkInterface()
        {
            var s = NetworkInterface.GetAllNetworkInterfaces();
            return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                     (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                     a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
        }

        public NetworkInterface GetVpnNetworkInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                     (a.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 && a.NetworkInterfaceType != NetworkInterfaceType.Ethernet && a.NetworkInterfaceType != NetworkInterfaceType.Loopback));
        }
        private async void checkConnection(object state)
        {
            var Nic = GetVpnNetworkInterface();
            if (Nic == null)
            {
                timer?.Change(Timeout.Infinite, Timeout.Infinite);
                if (timer != null)
                {
                    timer.Dispose();
                }

                await ChangeInterfaceMetric(false);
                connectionObserver.ConnectionObserver(false, "قطع");
            }
        }
    }
}
