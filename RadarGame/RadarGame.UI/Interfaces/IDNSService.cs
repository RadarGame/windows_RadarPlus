using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Interfaces
{
    public interface IDNSService
    {
        Task GetDataFromServerAndSetDNS();
        Task<bool> Connect();
        Task UnsetDNS();
        Task Dispose();
    }
}
