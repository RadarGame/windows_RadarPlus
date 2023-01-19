using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RadarGame.UI.Models;

namespace RadarGame.UI.Tools
{
    public interface IConnectionObserver
    {
        bool IsGettingData
        {
            get; 
            set;
        }

        ServerLocation GetVpnRegion();
        void ConnectionObserver(bool? SuccessfullyCoonected, string ServiceText);
    }
}
