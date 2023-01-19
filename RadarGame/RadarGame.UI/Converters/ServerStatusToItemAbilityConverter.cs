using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Newtonsoft.Json;
using RadarGame.UI.Models;
using RadarGame.UI.Tools;

namespace RadarGame.UI.Converters
{
    public class ServerStatusToItemAbilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;
            ServerLocation location = (ServerLocation)value;
            var serversStatusString = RadarHttpClient.GetInstance().Client.GetStringAsync(RadarUrls.ServersStatus).ContinueWith(
                (task) =>
                {
                    if (task.IsCompleted)
                    {
                        var serversStatus = JsonConvert.DeserializeObject<ServersState>(task.Result);
                        if (serversStatus != null)
                        {
                            switch (location)
                            {
                                case ServerLocation.Tehran:
                                    result = serversStatus.tehran;
                                    break;
                                case ServerLocation.Tabriz:
                                    result = serversStatus.tabriz;
                                    break;
                                case ServerLocation.Ahvaz:
                                    result = serversStatus.ahvaz;
                                    break;
                                case ServerLocation.Isfahan:
                                    result = serversStatus.isfahan;
                                    break;
                                case ServerLocation.Mashhad:
                                    result = serversStatus.mashhad;
                                    break;
                            }
                        }
                    }
                });
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
