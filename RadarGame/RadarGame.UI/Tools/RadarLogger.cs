using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Tools
{
    internal sealed class RadarLogger
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static RadarLogger _instance;
        private RadarLogger() { }

        public static RadarLogger GetInstance()
        {
            if (_instance == null)
                _instance = new RadarLogger();
            return _instance;
        }

        public NLog.Logger Logger
        {
            get => logger;
            set => logger = value;
        }
    }
}
