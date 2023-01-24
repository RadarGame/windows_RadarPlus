using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceId;
using Newtonsoft.Json;
using RadarGame.UI.Interfaces;
using RadarGame.UI.Models;
using RadarGame.UI.Tools;

namespace RadarGame.UI.Services
{
    public class WireGuard : IService
    {
        private IConnectionObserver connectionObserver;
        private ISettingsService settingsService;
        private VpnNetworkManager vpnNetworkManager;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private static readonly string userDirectory = Path.Combine(DirectoryHelperFunctions.GetTemporaryDirectory());
        private static readonly string configFile = Path.Combine(userDirectory, "RadarPlus.conf");

        public string ServiceText
        {
            get => "...درحال اتصال";
        }

        public WireGuard(IConnectionObserver connectionObserver,
            ISettingsService settingsService,
            VpnNetworkManager vpnNetworkManager)
        {
            this.connectionObserver = connectionObserver;
            this.settingsService = settingsService;
            this.vpnNetworkManager = vpnNetworkManager;
        }
        public async Task<bool> Connect()
        {
            try
            {
                var processInfo = await getWireGuardInfo();
                Process wireguard = new Process
                {
                    StartInfo = processInfo,
                    EnableRaisingEvents = true
                };
                wireguard.Start();
                connectionObserver.ConnectionObserver(null, "...بررسی اتصال");
                bool connected = false;
                await Task.Factory.StartNew(async () => connected = await PingHost("10.8.0.1", tokenSource.Token));
                if (connected)
                {
                    await vpnNetworkManager.ChangeInterfaceMetric(true);
                    connectionObserver.ConnectionObserver(true, "وصل");
                    //vpnNetworkManager.StartVpnConnectionObserver();
                    string deviceId = new DeviceIdBuilder().AddMacAddress().AddUserName().ToString();
                    // Provided way to count individual users via connecting.
                    Task.Factory.StartNew(() =>
                        RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.HardwareID + "Radar-" + deviceId));
                    Task.Factory.StartNew(() => connectionChecker("10.8.0.1", tokenSource.Token));
                    Task.Factory.StartNew(() =>
                        RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.AddUserToServerObserver));
                    return true;
                }
                await StopProcess();
                connectionObserver?.ConnectionObserver(false, "قطع");
                return false;
            }
            catch (Exception ex)
            {
                RadarLogger.GetInstance().Logger.Info(ex.Message);
                try { File.Delete(configFile); } catch { }

                return false;
            }
        }

        public async Task Dispose()
        {
            try
            {
                tokenSource.Cancel();
                tokenSource = new CancellationTokenSource();
                connectionObserver?.ConnectionObserver(null, "...درحال قطع اتصال");
                await StopProcess();
                await vpnNetworkManager.ChangeInterfaceMetric(false);
                Task.Factory.StartNew(() =>
                    RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.RemoveUserToServerObserver));
                vpnNetworkManager.StopVpnConnectionObserver();
            }
            catch (Exception e)
            {
                RadarLogger.GetInstance().Logger.Error(e);
            }
            finally
            {
                connectionObserver?.ConnectionObserver(false, "قطع");
            }
        }

        private async Task StopProcess()
        {
            await Task.Factory.StartNew(() =>
            {
                ProcessStartInfo wireGuardInfo = new ProcessStartInfo
                {
                    Arguments = "/uninstalltunnelservice RadarPlus",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    FileName = AppContext.BaseDirectory + "Wireguard\\wireguard.exe",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                Process wireguard = new Process
                {
                    StartInfo = wireGuardInfo,
                    EnableRaisingEvents = true
                };
                wireguard.Start();
            });
        }
        private async Task<ProcessStartInfo> getWireGuardInfo()
        {
            var region = Enum.GetName(typeof(ServerLocation), connectionObserver.GetVpnRegion());
            region = region.ToLower();
            await checkPrivateInfos(region);
            var publicProfileString = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.VpnRegionPublicInfo(region));
            var publicProfile = JsonConvert.DeserializeObject<WireguardJsons.PublicInfo>(publicProfileString);
            var routesjson = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.RadarRoutes);
            var Routes = JsonConvert.DeserializeObject<WireguardJsons.Routes>(routesjson);
            string routedata = "";
            Routes.routes.ToArray();
            foreach (string i in Routes.routes)
            {
                routedata += i + ",";
            }
            await generateConfig(routedata, publicProfile, region);
            string arguments = "/installtunnelservice " + configFile;
            ProcessStartInfo wireGuardInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = AppContext.BaseDirectory + "Wireguard\\wireguard.exe",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            return wireGuardInfo;
        }
        private async Task getWireGuardPrivateInfo(string region)
        {
            var infoString = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.VpnRegionPrivateInfo(region));
            var info = JsonConvert.DeserializeObject<WireguardJsons.PrivateInfo>(infoString);
            if (info != null)
            {
                var item = settingsService.Current.RegionPrivateInfos.FirstOrDefault(s => s.region == region);
                if (item != null)
                {
                    settingsService.Current.RegionPrivateInfos.Remove(item);
                }

                var privateInfo = new WireguardJsons.PrivateInfo {
                    region = region, 
                    ip = info.ip, 
                    psk = info.psk, 
                    private_key = info.private_key
                };
                settingsService.Current.RegionPrivateInfos.Add(privateInfo);
                await settingsService.SaveAsync();
            }
        }
        private async Task checkPrivateInfos(string region)
        {
            var connectionStateString = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.VpnRegionPublicInfo(region));
            var connectionState = JsonConvert.DeserializeObject<WireguardJsons.PublicInfo>(connectionStateString);
            if (settingsService.Current.WireguardConnectionState != connectionState?.changeState)
            {
                settingsService.Current.RegionPrivateInfos = new List<WireguardJsons.PrivateInfo>();
                await settingsService.SaveAsync();
                settingsService.Current.WireguardConnectionState = connectionState.changeState;
            }
            if (!settingsService.Current.RegionPrivateInfos.Any(s => s.region == region))
            {
                settingsService.Current.WireguardConnectionState = connectionState.changeState;
                await getWireGuardPrivateInfo(region);
            }
        }

        private async Task generateConfig(string Routes ,WireguardJsons.PublicInfo wgPublic, string region)
        {
            try
            {
                File.Delete(configFile);
            }
            catch { }

            var privateRegionInfo = settingsService.Current.RegionPrivateInfos.FirstOrDefault(s => s.region == region);
            var config = $"[Interface]\nPrivateKey = {privateRegionInfo.private_key}\nAddress = {privateRegionInfo.ip}\nDNS = {wgPublic.dns}\n\n[Peer]\nPublicKey = {wgPublic.publickey}\nPresharedKey =  {privateRegionInfo.psk}\nEndpoint = {wgPublic.endpoint}\nAllowedIPs = {Routes + wgPublic.routes }";
            using (var configurationFile = File.Open(configFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var configBytes = Encoding.UTF8.GetBytes(config);
                configurationFile.Seek(0, SeekOrigin.End);
                await configurationFile.WriteAsync(configBytes, 0, configBytes.Length);
            }
        }

        private async Task<bool> PingHost(string nameOrAddress, CancellationToken token)
        {
            bool pingable = false;
            Ping pinger = null;
            pinger = new Ping();
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        return false;
                    }
                    
                    PingReply reply = pinger.Send(nameOrAddress);
                    if (reply.Status == IPStatus.Success)
                    {
                        pingable = true;
                    }
                }
            }
            catch (PingException e)
            {
                RadarLogger.GetInstance().Logger.Error(e);
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }

        public async Task connectionChecker(string nameOrAddress, CancellationToken token)
        {
            Ping pinger = new Ping();
            int timeoutCounter = 0;
            int PingCounter = 0;
            try
            {
                while (true)
                {
                    //Adjust task.delay time according to spec of servers and amount of active users | Dont DDOS servers
                    await Task.Delay(1000);
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    
                    PingReply reply = pinger.Send(nameOrAddress,2000);
                    if (reply.Status != IPStatus.Success)
                    {
                        timeoutCounter++;
                    }
                    else if (reply.Status == IPStatus.Success)
                    {
                        timeoutCounter = 0;
                    }
                    PingTimer++;
                    if (timeoutCounter == 3)
                    {
                        // leave it there if its important if its not remove it Because of optimization
                        vpnNetworkManager.StartVpnConnectionObserver();
                        await Dispose();
                        return;
                    }
                    //Adjust task.delay time according to spec of servers and amount of active users
                    if (timeoutCounter==0 && PingTimer == 3) { PingTimer = 0; await Task.Delay(10000);}
                }
            }
            catch (PingException e)
            {
                RadarLogger.GetInstance().Logger.Error(e);
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }
        }
    }
}
