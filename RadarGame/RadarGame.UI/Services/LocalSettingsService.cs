using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using RadarGame.UI.Interfaces;
using RadarGame.UI.Models;

namespace RadarGame.UI.Services
{
    public class LocalSettingsService : ISettingsService
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private UserSettings userSettings;
        private static Mutex mutex;
        private string settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RadarGame", "user.config");
        private XmlSerializer serializer;

        public LocalSettingsService()
        {
            userSettings = new UserSettings();
            userSettings.SetDefaults();

            mutex = new Mutex(false, "RadarGame.UI.Services.LocalSettingsService");

            serializer = new XmlSerializer(typeof(UserSettings));
        }

        public UserSettings Current
        {
            get
            {
                return userSettings;
            }
            set
            {
                userSettings = value;
            }
        }
        public event EventHandler<UserSettings> SettingsSaved;

        public Task LoadAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    mutex.WaitOne();
                    if (File.Exists(settingsPath))
                    {
                        using (XmlReader reader = XmlReader.Create(settingsPath))
                        {
                            userSettings = (UserSettings)serializer.Deserialize(reader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            });
        }

        public Task SaveAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    mutex.WaitOne();
                    if (!Directory.Exists(Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RadarGame")))
                    {
                        Directory.CreateDirectory(Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RadarGame"));
                    }
                    using (StreamWriter writer = new StreamWriter(settingsPath))
                    {
                        serializer.Serialize(writer, userSettings);
                    }

                    SettingsSaved?.Invoke(this, Current);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            });
        }
    }
}
