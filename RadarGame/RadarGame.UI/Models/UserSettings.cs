using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RadarGame.UI.Tools;

namespace RadarGame.UI.Models
{
    public class UserSettings
    {
        private string privateKey;
        private string preSharedKey;
        private string endPoints;
        private string privateIps;
        private int wireguardConnectionState;
        private bool wGPrivateInfoObtained;
        private List<WireguardJsons.PrivateInfo> regionPrivateInfos;

        public string PrivateKey
        {
            get => privateKey;
            set => privateKey = value;
        }

        public string PreSharedKey
        {
            get => preSharedKey;
            set => preSharedKey = value;
        }

        public string EndPoints
        {
            get => endPoints;
            set => endPoints = value;
        }

        public string PrivateIps
        {
            get => privateIps;
            set => privateIps = value;
        }

        public bool WGPrivateInfoObtained
        {
            get => wGPrivateInfoObtained;
            set => wGPrivateInfoObtained = value;
        }

        public int WireguardConnectionState
        {
            get => wireguardConnectionState;
            set => wireguardConnectionState = value;
        }

        public List<WireguardJsons.PrivateInfo> RegionPrivateInfos
        {
            get => regionPrivateInfos;
            set => regionPrivateInfos = value;
        }
        public void SetDefaults()
        {
            PrivateKey = "";
            PreSharedKey = "";
            EndPoints = "";
            PrivateIps = "";
            WGPrivateInfoObtained = false;
            WireguardConnectionState = 0;
            RegionPrivateInfos = new List<WireguardJsons.PrivateInfo>();
        }
    }
}
