using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RadarGame.UI.Interfaces;
using RadarGame.UI.Models;
using RadarGame.UI.Services;
using RadarGame.UI.Tools;

namespace RadarGame.UI.ViewModels.Connections
{
    public class ProtocolsViewModel : BaseModel, IConnectionObserver
    {
        //Fields
        private string serviceText;
        private string protocolType;
        private bool configObtained;
        private bool isGettingData;
        private bool isTurnedOn;
        private bool isSliderOpen;
        private bool isOpenVpn;
        private bool isDnsSet;
        private bool canChangeServiceType = true;
        private ServerLocation selectedLocation;

        private IService service;
        private IDNSService dnsService;
        private IServiceProvider serviceProvider;

        ///Commands
        private RelayCommand configureDnsCommand;
        private RelayCommand setDnsCommand;
        private RelayCommand changeProtocolCommand;

        public Action<bool> ServiceUpdated;
        //public Action<bool> FreezeForm;
        private bool isEnableToChangeService = true;

        #region Properties(Getter, Setter)

        public bool ConfigObtained
        {
            get => configObtained;
            set
            {
                configObtained = value;
                OnPropertyChanged();
            }
        }

        public bool IsSliderOpen
        {
            get => isSliderOpen;
            set
            {
                isSliderOpen = value;
                OnPropertyChanged();
            }
        }

        public bool IsOpenVpn
        {
            get => isOpenVpn;
            set
            {
                isOpenVpn = value;
                ChangeModel(value ? "Wireguard" : "OpenVPN");
                OnPropertyChanged();
            }
        }
        public string ProtocolType
        {
            get => protocolType;
            set
            {
                protocolType = value;
                OnPropertyChanged();
            }
        }
        public bool IsGettingData
        {
            get => isGettingData;
            set
            {
                isGettingData = value;
                OnPropertyChanged();
            }
        }

        public bool IsTurnedOn
        {
            get => isTurnedOn;
            set
            {
                isTurnedOn = value;
                OnPropertyChanged();
            }
        }

        public string ServiceText
        {
            get
            {
                return serviceText;
            }
            set
            {
                serviceText = value;
                OnPropertyChanged();
            }
        }

        public bool IsDnsSet
        {
            get => isDnsSet;
            set
            {
                isDnsSet = value;
                if (isDnsSet)
                {
                    Application.Current.Dispatcher.InvokeAsync(dnsService.Connect);
                }
                else
                {
                    Application.Current.Dispatcher.InvokeAsync(dnsService.Dispose);
                }
            }
        }

        public ServerLocation SelectedLocation
        {
            get => selectedLocation;
            set
            {
                selectedLocation = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ServerLocation> ServerLocations
        {
            get => Enum.GetValues(typeof(ServerLocation)).Cast<ServerLocation>();
        }
        public RelayCommand ConfigureDnsCommand => configureDnsCommand ??
                                                   (configureDnsCommand = new RelayCommand(Connect));
        public RelayCommand SetDnsCommand => setDnsCommand ??
                                             (setDnsCommand = new RelayCommand(SetDns));
        public RelayCommand ChangeProtocolCommand => changeProtocolCommand ??
                                             (changeProtocolCommand = new RelayCommand(changeProtocol));
        public bool IsEnableToChangeService
        {
            get => isEnableToChangeService;
            set
            {
                isEnableToChangeService = value;
                //FreezeForm?.Invoke(value);
                OnPropertyChanged();
            }

        }
        #endregion

        //Constructor
        public ProtocolsViewModel(
            IServiceProvider serviceProvider,
            IDNSService dnsService)
        {
            serviceText = "قطع";
            this.serviceProvider = serviceProvider;
            this.dnsService = dnsService;
            selectedLocation = ServerLocation.Tehran;
        }

        #region Private Methods

        //Set DNS and start to connect to service.
        private async void Connect(object obj)
        {
            IsSliderOpen = false;
            if (IsGettingData == true && IsTurnedOn == false)
            {
                await service.Dispose();
                ServiceText = "قطع";
                IsTurnedOn = false;
                IsGettingData = false;
                IsEnableToChangeService = true;
            }
            else if (IsTurnedOn == false)
            {
                if (service == null)
                {
                    ChangeModel("");
                }
                IsGettingData = true;
                ServiceText = service.ServiceText;
                IsEnableToChangeService = false;
                bool result = await service.Connect();

                if (!result)
                {
                    IsEnableToChangeService = true;
                    IsGettingData = false;
                    IsTurnedOn = false;
                    ServiceText = "خطا";
                }

            }
            else
            {
                await service.Dispose();
                ServiceText = "قطع";
                IsTurnedOn = false;
                IsEnableToChangeService = true;
            }
        }

        private async void SetDns(object obj)
        {
           // await dnsService.Connect();
        }

        private void changeProtocol(object obj)
        {
            var type = (string)obj;
            ChangeModel(type);
        }
        #endregion

        #region Public Methods
        public static void UnsetDnsEvent()
           => DNSService.UnsetDnsEvent();

        public void ChangeModel(string service)
        {
            switch (service)
            {
                case "OpenVPN":
                    this.service = serviceProvider.GetRequiredService<OpenVPN>();
                    ProtocolType = service;
                    break;
                case "PPTP":
                    this.service = serviceProvider.GetRequiredService<PPTP>();
                    ProtocolType = service;
                    break;
                case "Wireguard":
                    ProtocolType = service;
                    this.service = serviceProvider.GetRequiredService<WireGuard>();
                    break;
                default:
                    this.service = serviceProvider.GetRequiredService<WireGuard>();
                    ProtocolType = "Wireguard";
                    break;
            }
        }

        public void ConnectionObserver(bool? SuccessfullyCoonected, string serviceText)
        {

            if (SuccessfullyCoonected != null)
            {
                if (SuccessfullyCoonected ?? false)
                {
                    IsGettingData = false;
                    IsTurnedOn = true;
                    this.ServiceText = serviceText;
                    ServiceUpdated?.Invoke(true);
                    IsEnableToChangeService = false;
                }
                else
                {
                    IsGettingData = false;
                    IsTurnedOn = false;
                    this.ServiceText = serviceText;
                    ServiceUpdated?.Invoke(false);
                    IsEnableToChangeService = true;
                }

            }
            else
            {
                this.ServiceText = serviceText;
            }
        }

        public ServerLocation GetVpnRegion() => SelectedLocation;

        #endregion
    }
}
