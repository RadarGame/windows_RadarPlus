using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Tools
{
    internal static class RadarUrls
    {
        internal static readonly string PptpIp = "https://cdn.radar.game/app/vpn/radarpptp.txt";

        internal static readonly string HardwareID = "server_ip"; // for security reasons must not shown in public.;

        internal static readonly string Route12Bat = "https://cdn.radar.game/app/vpn/routes/route12.bat";

        internal static readonly string RouteDel = "https://cdn.radar.game/app/vpn/routes/routedel.bat";

        internal static readonly string WGPrivateInfo = "server_ip"; // for security reasons must not shown in public.

        internal static readonly string WireGuardConfig = "https://cdn.radar.game/app/vpn/wireguardJson.json";

        internal static readonly string VpnRegions = "https://cdn.radar.game/app/vpn/vpnregion.json";

        internal static readonly string SettingsJson = "https://cdn.radar.game/app/radar.json";

        internal static readonly string RadarRoutes = "https://cdn.radar.game/app/vpn/radarroute.json";

        internal static readonly string OpenVpn = "https://cdn.radar.game/app/vpn/radar.ovpn";

        internal static readonly string OpenVpnUser = "https://cdn.radar.game/app/vpn/user.cfg";

        internal static readonly string RadarSites = "https://cdn.radar.game/app/links.json";

        internal static readonly string Version = "https://cdn.radar.game/dl/pc/Radarupdate.xml";

        internal static readonly string ServersStatus = "https://cdn.radar.game/app/vpn/servers_state.json";

        internal static readonly string AddUserToServerObserver = "server_ip"; // for security reasons must not shown in public.;

        internal static readonly string RemoveUserToServerObserver = "server_ip"; // for security reasons must not shown in public.;

        internal static string VpnRegionPrivateInfo(string region)
        {
            return $"server_ip"; // for security reasons must not shown in public.;
        }
        internal static string VpnRegionPublicInfo(string region)
        {
            return $"https://cdn.radar.game/app/vpn/{region}wireguard.json";
        }
    }
}
