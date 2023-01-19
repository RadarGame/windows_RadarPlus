using RadarGame.UI.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RadarGame.UI.Interfaces;
using RadarGame.UI.Models;
using RadarGame.UI.Tools;

namespace RadarGame.UI.Services
{
    public class Softether : IService
    {
        public string ServiceText => "";

        public async Task<bool> Connect()
        {
            return true;
        }

        public async Task Dispose()
        {
        }
    }
}
