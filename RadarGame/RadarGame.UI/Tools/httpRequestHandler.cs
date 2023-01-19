using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Tools
{
    public class HttpRequestHandler
    {
        private const string serverPath = "https://cdn.radar.game/app/vpn/";

        private const string userCheckRegistry = "check_user";
        private const string addNewUser = "new_user";
        private const string change_pass = "change_pass";
        private const string softEtherConfig = "softether.json";

        private static string SendGetRequest(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private async static Task<string> SendPostRequest(string uri, string uniqueId = "", string pass = "", string username = "")
        {
            var values = new Dictionary<string, string>();

            if (uniqueId != "")
                values.Add("UniqueId", uniqueId);
            if (pass != "")
                values.Add("Password", uniqueId);
            if (username != "")
                values.Add("Username", uniqueId);

            var content = new FormUrlEncodedContent(values);

            var response = await RadarHttpClient.GetInstance().Client.PostAsync(uri, content);

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public static async Task<string> SoftEtherConfigRequestAndGetResponse(/*string uniqueId*/)
        {
            string uri = serverPath + softEtherConfig;
            //return await SendPostRequest(uri/*, uniqueId*/);
            return SendGetRequest(uri/*, uniqueId*/);
        }

        public static async Task<string> SendCheckUserRegistryRequestAndGetResponse(string uniqueId, string pass)
        {
            string uri = serverPath + userCheckRegistry;
            return await SendPostRequest(uri, uniqueId, pass);
        }

        public static async Task<string> NewUserSignupRequestAndGetResponse(string uniqueId, string pass, string username)
        {

            string uri = serverPath + addNewUser;

            return await SendPostRequest(uri, uniqueId, pass, username);
        }

        public static async Task<string> ChangePassword(string uniqueId, string pass)
        {
            string uri = serverPath + change_pass;

            return await SendPostRequest(uri, uniqueId, pass);
        }
    }
}
