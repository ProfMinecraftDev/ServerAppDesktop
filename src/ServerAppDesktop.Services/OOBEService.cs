using System.IO;
using System.Text.Json;
using Microsoft.Windows.AppLifecycle;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Services
{
    public sealed class OOBEService : IOOBEService
    {
        public void SaveUserSettings(AppSettings appSettings)
        {
            DataHelper.Settings = appSettings;

            var context = new AppSettingsJsonContext(new JsonSerializerOptions
            {
                WriteIndented = true,
                IndentSize = 4
            });

            string jsonString = JsonSerializer.Serialize(DataHelper.Settings, context.AppSettings);

            string jsonFilePath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);

            if (!string.IsNullOrEmpty(DataHelper.SettingsPath) && !Directory.Exists(DataHelper.SettingsPath))
                Directory.CreateDirectory(DataHelper.SettingsPath);

            File.WriteAllText(jsonFilePath, jsonString);
        }

        public void RestartApplication()
        {
            AppInstance.Restart("");
        }
    }
}
