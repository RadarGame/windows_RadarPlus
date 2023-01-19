using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using RadarGame.UI.Interfaces;
using RadarGame.UI.Services;
using RadarGame.UI.Tools;
using RadarGame.UI.ViewModels;
using RadarGame.UI.ViewModels.Connections;

namespace RadarGame.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider serviceProvider;

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Startup += App_Startup;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            RadarLogger.GetInstance().Logger.Error(e.Exception);
            //If we have a main window continue working unless let the exception go 
            //that will cause application close
            if (Current.MainWindow != null &&
                Current.MainWindow.IsInitialized &&
                Current.MainWindow.IsLoaded)
            {
                e.Handled = true;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            RadarLogger.GetInstance().Logger.Error(e.ExceptionObject);
            RadarLogger.GetInstance().Logger.Error("Shutting down due to an unhandled exception.");
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "RadarLog.txt" };
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;

            var runningInstances = System.Diagnostics.Process.GetProcessesByName(
                    System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (runningInstances.Length > 1)
            {
                Environment.Exit(0);
            }

            var serviceProvider = configureServices();
            this.serviceProvider = serviceProvider;
            initServices(serviceProvider);
        }

        private void initServices(IServiceProvider serviceProvider)
        {
            var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
            settingsService.LoadAsync();
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            Current.MainWindow = mainWindow;

            mainWindow.DataContext = serviceProvider.GetRequiredService<MainViewModel>();

            mainWindow.Show();
        }


        private IServiceProvider configureServices()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ISettingsService, LocalSettingsService>();
            serviceCollection.AddSingleton<IDNSService, DNSService>();
            serviceCollection.AddSingleton<ProtocolsViewModel>();
            serviceCollection.AddSingleton<IConnectionObserver>(s => s.GetRequiredService<ProtocolsViewModel>());
            serviceCollection.AddSingleton<VpnNetworkManager>();
            serviceCollection.AddSingleton<WireGuard>();
            serviceCollection.AddScoped<MainViewModel>();
            serviceCollection.AddScoped<MainWindow>();

            return serviceCollection.BuildServiceProvider();

        }
        private async void Application_Exit(object sender, ExitEventArgs e)
        {

            var dnsService = serviceProvider.GetRequiredService<IDNSService>();
            var wireguardService = serviceProvider.GetRequiredService<WireGuard>();
            await dnsService.UnsetDNS();

            wireguardService.Dispose();
        }
    }
}
