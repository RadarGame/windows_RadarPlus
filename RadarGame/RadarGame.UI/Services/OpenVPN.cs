using DeviceId;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RadarGame.UI.Interfaces;
using RadarGame.UI.Tools;

namespace RadarGame.UI.Services
{
    public class OpenVPN : IService
    {
        //Fields
        IConnectionObserver connectionObserver;
        private VpnNetworkManager networkManager;
        public string ServiceText => "...درحال اتصال"; 

        //Constructor
        public OpenVPN(IConnectionObserver connectionObserver,
            VpnNetworkManager networkManager)
        {
            this.connectionObserver = connectionObserver;
            this.networkManager = networkManager;
        }

        #region Private Methods
        private async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                if (!connectionObserver.IsGettingData)
                {
                    await StopProcess();
                    return;
                }
                string output = outLine.Data;
                RadarLogger.GetInstance().Logger.Info(output);

                if (output != null)
                {
                    if (output.Contains("Initialization Sequence Completed"))
                    {
                        // this may lead to error duo to connection error and does not show the user that the vpn connection succeed.
                        await networkManager.ChangeInterfaceMetric(true);
                        connectionObserver?.ConnectionObserver(true, "وصل");
                        networkManager.StartVpnConnectionObserver();
                    }
                    else if (output.Contains("TLS Error: TLS handshake failed"))
                    {
                        await StopProcess();
                        connectionObserver?.ConnectionObserver(false, "خطا");
                    }
                    else if (output.Contains("fatal error"))
                    {
                        await StopProcess();
                        connectionObserver?.ConnectionObserver(false, "خطا");
                    }
                }
            }
            catch (Exception e)
            {
                RadarLogger.GetInstance().Logger.Error(e);
            }
        }

        private async Task<string> CreateBatchAndGetPath()
        {
            string newTempDir = DirectoryHelperFunctions.GetTemporaryDirectory();
            string pathToRouteBatch = newTempDir + "//route12.bat";
            var routeBatch = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.Route12Bat);
            await DirectoryHelperFunctions.WriteAsync(routeBatch, pathToRouteBatch);
            return pathToRouteBatch;
        }

        private async Task RunCMDCode(string pathToRouteBatch)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.FileName = @"cmd.exe";
            psi.Verb = "runas";

            psi.Arguments = "/C \"" + pathToRouteBatch;

            Process proc = new Process();
            proc.StartInfo = psi;

            proc.Start();
            string deviceId = new DeviceIdBuilder().AddMacAddress().AddUserName().ToString();
            await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.HardwareID + deviceId);
        }

        private async Task<ProcessStartInfo> GetOvpnInfo()
        {
            string newTempDir = DirectoryHelperFunctions.GetTemporaryDirectory();
            string pathToConfig = newTempDir + "//profile.ovpn";
            string pathToConfigcfg = "C://Windows" + "//user.cfg";
            var openVpnProfile = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.OpenVpn);
            var openVpnProfileconf = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.OpenVpnUser);
            await DirectoryHelperFunctions.WriteAsync(openVpnProfile, pathToConfig);
            await DirectoryHelperFunctions.WriteAsync(openVpnProfileconf, pathToConfigcfg);
            string arguments = "--config \"" + pathToConfig + "\" --block-outside-dns";
            ProcessStartInfo openVpnStartInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = AppContext.BaseDirectory + "/Openvpn/openvpn.exe",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            return openVpnStartInfo;
        }

        private async Task StopProcess()
        {
            var openVpnProcess = Process.GetProcesses().
                Where(pr => pr.ProcessName == "openvpn");

            foreach (var process in openVpnProcess)
                process.Kill();

            await networkManager.ChangeInterfaceMetric(false);
        }


        #endregion

        #region Public Methods
        public async Task<bool> Connect()
        {
            try
            {
                ///Start OpenVPN EXE.
                ProcessStartInfo openVpnStartInfo = await GetOvpnInfo();
                Process openVpn = new Process
                {
                    StartInfo = openVpnStartInfo,
                    EnableRaisingEvents = true
                };
                openVpn.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                openVpn.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                openVpn.Start();
                openVpn.BeginOutputReadLine();
                openVpn.BeginErrorReadLine();
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
            try
            {
                connectionObserver?.ConnectionObserver(null, "...درحال قطع اتصال");
                await StopProcess();
                networkManager.StopVpnConnectionObserver();
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
        #endregion
    }
}
