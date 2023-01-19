using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Tools
{

    public class Rootobject
    {
        public string dns1
        {
            get; set;
        }
        public string dns2
        {
            get; set;
        }
    }

    public class Version
    {
        public string lastVersion
        {
            get; set;
        }
        public string downloadPath
        {
            get; set;
        }
    }


    public class Route
    {
        public string[] routes
        {
            get; set;
        }

        public string pptproute
        {
            get;
            set;
        }

        public string deleteroute
        {
            get;
            set;
        }
    }

    public class RadarSites
    {
        public string panco
        {
            get; set;
        }
        public string virasty
        {
            get; set;
        }
    }

    public class VpnRegions
    {
        public string[] regions
        {
            get;
            set;
        }
    }


    public class ServersState
    {
        public bool mashhad
        {
            get; set;
        }
        public bool isfahan
        {
            get; set;
        }
        public bool tabriz
        {
            get; set;
        }
        public bool ahvaz
        {
            get; set;
        }
        public bool tehran
        {
            get; set;
        }
    }

}
