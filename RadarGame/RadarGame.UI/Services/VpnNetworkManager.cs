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
        public async Task<bool> CreateRoutes(Route routeJson, bool isPptp, CancellationToken cancellationToken)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PMIB_IPFORWARDROW)));
            try
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(nics => nics.Name == MyConnection._ConnectionName);
                if (!isPptp)
                {
                    nic = GetVpnNetworkInterface();
                }

                uint netIndex = Convert.ToUInt32(nic.GetIPProperties().GetIPv4Properties().Index);
                var properties = nic.GetIPProperties();
                var primaryGateway = getPrimaryGateway(properties, nic.GetIPProperties().GetIPv4Properties().Index);
                int countAddedRoutes = 0;
                if (isPptp)
                {
                    await pptpExtraStep(routeJson.pptproute, routeJson.deleteroute, primaryGateway, netIndex);
                }
                foreach (string ipAddress in routeJson.routes)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    var splitedAddress = ipAddress.Split('/');
                    var route = new PMIB_IPFORWARDROW
                    {
                        dwForwardDest = BitConverter.ToUInt32(IPAddress.Parse(splitedAddress[0]).GetAddressBytes(), 0),
                        dwForwardMask =
                            BitConverter.ToUInt32(IPAddress.Parse(findMask(splitedAddress[1])).GetAddressBytes(), 0),
                        dwForwardNextHop =
                            BitConverter.ToUInt32(
                                IPAddress.Parse(primaryGateway.ToString())
                                    .GetAddressBytes(), 0),
                        dwForwardMetric1 = Convert.ToUInt32(1),
                        dwForwardType = Convert.ToUInt32(3), //Default to 3
                        dwForwardProto = Convert.ToUInt32(3), //Default to 3
                        dwForwardAge = 0,
                        dwForwardIfIndex = netIndex,
                    };
                    Marshal.StructureToPtr(route, ptr, false);
                    var status = NativeMethods.CreateIpForwardEntry(ptr);
                    connectionObserver.ConnectionObserver(null, $"● Added routes {countAddedRoutes++}/{routeJson.routes.Length}");
                    await Task.Delay(20);
                }

                return true;
            }
            catch (Exception e)
            {
                RadarLogger.GetInstance().Logger.Error(e);
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        public void DeleteRoute(string ipAddress, uint networkIndex, IPAddress primaryGateway, uint metric)
        {
            var splitedAddress = ipAddress.Split('/');
            var route = new MIB_IPFORWARDROW
            {
                dwForwardDest = BitConverter.ToUInt32(IPAddress.Parse(splitedAddress[0]).GetAddressBytes(), 0),
                dwForwardMask = BitConverter.ToUInt32(IPAddress.Parse(findMask(splitedAddress[1])).GetAddressBytes(), 0),
                dwForwardNextHop = BitConverter.ToUInt32(IPAddress.Parse(primaryGateway.ToString()).GetAddressBytes(), 0),
                dwForwardMetric1 = metric,
                dwForwardType = Convert.ToUInt32(3), //Default to 3
                dwForwardProto = Convert.ToUInt32(3), //Default to 3
                dwForwardAge = 0,
                dwForwardIfIndex = networkIndex
            };

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MIB_IPFORWARDROW)));
            try
            {
                Marshal.StructureToPtr(route, ptr, false);
                var status = NativeMethods.DeleteIpForwardEntry(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

        }

        public async Task DeleteRoutesFromBatFile()
        {
            var tempDirectory = DirectoryHelperFunctions.GetTemporaryDirectory();
            string pathToRouteBatch = tempDirectory + "//routedel.bat";
            var routeBatch = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.RouteDel);
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
        private List<RouteEntryModel> getRouteTable()
        {
            var fwdTable = IntPtr.Zero;
            int size = 0;
            var result = NativeMethods.GetIpForwardTable(fwdTable, ref size, true);
            fwdTable = Marshal.AllocHGlobal(size);

            result = NativeMethods.GetIpForwardTable(fwdTable, ref size, true);

            var forwardTable = NativeMethods.ReadIPForwardTable(fwdTable);

            Marshal.FreeHGlobal(fwdTable);


            List<RouteEntryModel> routeTable = new List<RouteEntryModel>();
            for (int i = 0; i < forwardTable.Table.Length; ++i)
            {
                RouteEntryModel entry = new RouteEntryModel();
                entry.DestinationIP = new IPAddress((long)forwardTable.Table[i].dwForwardDest);
                entry.SubnetMask = new IPAddress((long)forwardTable.Table[i].dwForwardMask);
                entry.GatewayIP = new IPAddress((long)forwardTable.Table[i].dwForwardNextHop);
                entry.InterfaceIndex = Convert.ToInt32(forwardTable.Table[i].dwForwardIfIndex);
                entry.ForwardType = Convert.ToInt32(forwardTable.Table[i].dwForwardType);
                entry.ForwardProtocol = Convert.ToInt32(forwardTable.Table[i].dwForwardProto);
                entry.ForwardAge = Convert.ToInt32(forwardTable.Table[i].dwForwardAge);
                entry.Metric = Convert.ToInt32(forwardTable.Table[i].dwForwardMetric1);
                routeTable.Add(entry);
            }
            return routeTable;
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

        private IPAddress getPrimaryGateway(IPInterfaceProperties properties, int networkIndex)
        {
            IPAddress primaryGateway = null;
            if (properties.GatewayAddresses.Count > 0)
            {
                foreach (GatewayIPAddressInformation gatewayInfo in properties.GatewayAddresses)
                {
                    if (gatewayInfo.Address != null && gatewayInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        primaryGateway = gatewayInfo.Address;
                        break;
                    }
                }
            }
            else
            {
                //if the gateways on the Network adaptor properties is null, then get it from the routing table, especially the case for VPN routers
                List<RouteEntryModel> routeTable = getRouteTable();
                if (routeTable.Where(i => i.InterfaceIndex == networkIndex)?.Count() > 0)
                {
                    primaryGateway = routeTable.Where(i => i.InterfaceIndex == networkIndex)?.First()?.GatewayIP;

                }
            }
            //not ideal and incorrect, but hopefully it doesn't execute this as the gateways are defined elsewhere
            //the correct way is to locate the primary gateway in some other property other than the 3 methods here
            if (primaryGateway == null && properties.DhcpServerAddresses.Count > 0)
            {
                primaryGateway = properties.DhcpServerAddresses.First();
            }

            return primaryGateway;
        }

        private async Task pptpExtraStep(string addRoute, string deleteRoute, IPAddress primaryGateway, uint netIndex)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PMIB_IPFORWARDROW)));
            try
            {
                var splitedAddress = addRoute.Split('/');
                var route = new PMIB_IPFORWARDROW
                {
                    dwForwardDest = BitConverter.ToUInt32(IPAddress.Parse(splitedAddress[0]).GetAddressBytes(), 0),
                    dwForwardMask =
                        BitConverter.ToUInt32(IPAddress.Parse(findMask(splitedAddress[1])).GetAddressBytes(), 0),
                    dwForwardNextHop =
                        BitConverter.ToUInt32(
                            IPAddress.Parse(primaryGateway.ToString())
                                .GetAddressBytes(), 0),
                    dwForwardMetric1 = Convert.ToUInt32(1),
                    dwForwardType = Convert.ToUInt32(3), //Default to 3
                    dwForwardProto = Convert.ToUInt32(3), //Default to 3
                    dwForwardAge = 0,
                    dwForwardIfIndex = netIndex
                };
                Marshal.StructureToPtr(route, ptr, false);
                var status = NativeMethods.CreateIpForwardEntry(ptr);
                await Task.Delay(20);

                DeleteRoute(deleteRoute, netIndex, primaryGateway, 2);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
