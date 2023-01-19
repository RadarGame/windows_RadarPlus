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
        private bool isStartup;
        private string sponsorImageUrl;
        private string sponsorLinkUrl;
        private readonly WindowState _windowState;
        private string selectedService = "ٌWireGuard";


        ///Commands
        private RelayCommand notifyCommand;
        private RelayCommand elTeamSiteCommand;
        private RelayCommand discordCommand;
        private RelayCommand telegramCommand;
        private RelayCommand instagramCommand;
        private RelayCommand donateCommand;
        private RelayCommand sponsorCommand;
        #region Properties(Getter, Setter)

        #region Commands 
        public RelayCommand NotifyCommand => notifyCommand ??
                                             (notifyCommand = new RelayCommand(Notify));

        public RelayCommand ElTeamSiteCommand => elTeamSiteCommand ??
                                                 (elTeamSiteCommand = new RelayCommand(ElTeamSite));

        public RelayCommand DiscordCommand => discordCommand ??
                                              (discordCommand = new RelayCommand(Discord));

        public RelayCommand TelegramCommand => telegramCommand ??
                                               (telegramCommand = new RelayCommand(Telegram));

        public RelayCommand InstagramCommand => instagramCommand ??
                                                (instagramCommand = new RelayCommand(Instagram));

        public RelayCommand DonateCommand => donateCommand ??
                                             (donateCommand = new RelayCommand(Donate));

        public RelayCommand SponsorCommand => sponsorCommand ??
                                              (sponsorCommand = new RelayCommand(Sponsor));
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

        public string SponsorImageUrl
        {
            get => sponsorImageUrl;
            set
            {
                sponsorImageUrl = value;
                OnPropertyChanged();
            }
        }
        public bool IsStartup
        {
            get => isStartup;
            set
            {
                isStartup = value;
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

        public IEnumerable<string> ServicesCombo
                => new string[] { "DNS Changer" };



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

        private void ElTeamSite(object obj) => Process.Start("http://www.radar.game/contact-us");
        private void Discord(object obj) => Process.Start("http://radar.game");

        private async void Telegram(object obj)
        {
            var linkString = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.RadarSites);
            var link = JsonConvert.DeserializeObject<RadarSites>(linkString).panco;
            Process.Start(link);
        }
        private async void Instagram(object obj)
        {
            var linkString = await RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.RadarSites);
            var link = JsonConvert.DeserializeObject<RadarSites>(linkString).virasty;
            Process.Start(link);
        }
        private void Donate(object obj) => Process.Start("https://donateon.ir/MaxisAmir");
        private void Sponsor(object obj) => Process.Start(sponsorLinkUrl);
        #endregion

        #endregion

        #region Public Methods

        #endregion
    }
}
