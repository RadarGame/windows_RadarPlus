using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using RadarGame.UI.Tools;
using RadarGame.UI.ViewModels.Connections;

namespace RadarGame.UI.ViewModels
{
    public class MainViewModel : BaseModel
    {
        //Fields
        private readonly ProtocolsViewModel protocolsViewModel;
        private NotifyIconWrapper.NotifyRequestRecord _notifyRequest;
        private bool isServiceOn;
        private bool _showInTaskbar;
        private readonly WindowState _windowState;


        ///Commands
        private RelayCommand notifyCommand;
        private RelayCommand contactCommand;
        private RelayCommand siteCommand;
        private RelayCommand pancoCommand;
        private RelayCommand virastyCommand;
        private RelayCommand donateCommand;
        private RelayCommand sponsorCommand;
        #region Properties(Getter, Setter)

        #region Commands 
        public RelayCommand NotifyCommand => notifyCommand ??
                                             (notifyCommand = new RelayCommand(Notify));

        public RelayCommand ContactCommand => contactCommand ??
                                                 (contactCommand = new RelayCommand(contact));

        public RelayCommand SiteCommand => siteCommand ??
                                              (siteCommand = new RelayCommand(site));

        public RelayCommand PancoCommand => pancoCommand ??
                                               (pancoCommand = new RelayCommand(panco));

        public RelayCommand VirastyCommand => virastyCommand ??
                                                (virastyCommand = new RelayCommand(virasty));
        #endregion

        public ProtocolsViewModel ProtocolsViewModel => protocolsViewModel;
        public bool IsServiceOn
        {
            get => isServiceOn;
            set
            {
                isServiceOn = value;
                OnPropertyChanged();
            }
        }
        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                ShowInTaskbar = true;
                OnPropertyChanged();
                ShowInTaskbar = value != WindowState.Minimized;
            }
        }

        public bool ShowInTaskbar
        {
            get => _showInTaskbar;
            set
            {
                _showInTaskbar = value;
                OnPropertyChanged();
            }
        }

        public NotifyIconWrapper.NotifyRequestRecord NotifyRequest
        {
            get => _notifyRequest;
            set
            {
                _notifyRequest = value;
                OnPropertyChanged();
            }
        }

        public string SelectedService
        {
            get => selectedService;
            set
            {
                if (selectedService != value)
                {
                    protocolsViewModel.ChangeModel(value);
                    selectedService = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion
        //Constructor
        public MainViewModel(ProtocolsViewModel protocolsViewModel)
        {
            this.protocolsViewModel = protocolsViewModel;
            protocolsViewModel.ServiceUpdated += ServiceUpdated;

        }

        #region Private Methods

        #region Command Handler
        private void Notify(object obj)
        {
            if (ProtocolsViewModel.IsTurnedOn)
            {
                NotifyRequest = new NotifyIconWrapper.NotifyRequestRecord
                {
                    Title = "Radar Game",
                    Text = "سرویس رادار هنوز در حال اجراست!",
                    Duration = 1000
                };
            }
        }
        private void ServiceUpdated(bool isTurnedOn)
        {
            string description;
            if (isTurnedOn)
            {
                description = "سرویس رادار پلاس فعال شد.";
            }
            else
            {
                description = "سرویس رادار پلاس غیر فعال شد.";
            }
            NotifyRequest = new NotifyIconWrapper.NotifyRequestRecord
            {
                Title = "Radar Game",
                Text = description,
                Duration = 1000
            };
        }
        //private void FreezeForm(bool freezeForm)
        //    => this.FormEnable = !freezeForm;

        private void contact(object obj) => Process.Start("http://www.radar.game/contact-us");
        private void site(object obj) => Process.Start("http://radar.game");

        private async void panco(object obj)
        {
            var linkString = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.RadarSites);
            var link = JsonConvert.DeserializeObject<RadarSites>(linkString).panco;
            Process.Start(link);
        }
        private async void virasty(object obj)
        {
            var linkString = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.RadarSites);
            var link = JsonConvert.DeserializeObject<RadarSites>(linkString).virasty;
            Process.Start(link);
        }
        #endregion

        #endregion

        #region Public Methods

        #endregion
    }
}
