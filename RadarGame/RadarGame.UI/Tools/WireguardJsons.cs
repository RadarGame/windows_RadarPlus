using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Tools
{
    public class WireguardJsons
    {
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public class PrivateInfo
        {
            public string region { get; set; }
            public string ip { get; set; }
            public string private_key { get; set; }
            public string psk { get; set; }
        }
        
        public class PublicInfo
        {
            public int changeState { get; set; }
            public string publickey { get; set; }
            public string endpoint { get; set; }
            public string dns { get; set; }
            public string routes { get; set; }
        }

    }
}
