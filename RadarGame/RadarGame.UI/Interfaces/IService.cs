using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RadarGame.UI.Interfaces
{
    public interface IService
    {
        string ServiceText { get;}
        Task<bool> Connect();
        Task Dispose();
    }
}
