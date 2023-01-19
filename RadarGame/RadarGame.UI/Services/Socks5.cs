using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RadarGame.UI.Interfaces;

namespace RadarGame.UI.Services
{
    internal class Socks5 : IService
    {
        public string ServiceText => "● Connecting to Socks5...";

        public async Task<bool> Connect()
        {
            try
            {
                //var client = new HttpClient(new SocketsHttpHandler()
                //{
                //    Proxy = new WebProxy("socks5://127.0.0.1:9050")
                //});

                //var content = await client.GetStringAsync("https://check.torproject.org/");

                //Console.WriteLine(content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task Dispose()
        {
        }
    }
}
