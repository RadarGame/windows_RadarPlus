using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Tools
{
    public sealed class RadarHttpClient
    {
        private static RadarHttpClient _instance;
        private HttpClient client = new HttpClient();

        private RadarHttpClient() { }

        public static RadarHttpClient GetInstance()
        {
            if (_instance == null)
            {
                _instance = new RadarHttpClient();
            }
            return _instance;
        }

        public HttpClient Client
        {
            get => client;
            set => client = value;
        }
    }
}
