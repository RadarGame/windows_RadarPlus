using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Models
{
    public class RouteEntryModel
    {
        public IPAddress DestinationIP
        {
            get; set;
        }
        public IPAddress SubnetMask
        {
            get; set;
        }
        public IPAddress GatewayIP
        {
            get; set;
        }
        public int InterfaceIndex
        {
            get; set;
        }
        public int ForwardType
        {
            get; set;
        }
        public int ForwardProtocol
        {
            get; set;
        }
        public int ForwardAge
        {
            get; set;
        }
        public int Metric
        {
            get; set;
        }
    }
}
