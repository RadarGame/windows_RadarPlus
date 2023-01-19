using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Tools
{
    public static class OpenVPNConnectionDescription
    {
        public static Dictionary<string, string> Description = new Dictionary<string, string>()
        {
            ["TCP/UDP:"] = "Initializing",
            ["TLS: Initial packet"] = "Verifying Connection",
            ["Peer Connection Initiated"] = "Starting Connection",
            ["open_tun"] = "Creating TAP Device",
            ["TEST ROUTES:"] = "Testing routes"
        };
    }
}
