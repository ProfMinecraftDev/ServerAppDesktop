using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Services
{
    public sealed class OOBEService : IOOBEService
    {
        public event Action<bool>? OOBEFinished;
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

            if (appSettings.Server.Edition == 1) // Java Edition
            {
                // Extraemos la carpeta donde vive el .jar
                string? serverFolder = Path.GetDirectoryName(appSettings.Server.Executable);

                if (!string.IsNullOrEmpty(serverFolder))
                {
                    string eulafilePath = Path.Combine(serverFolder, "eula.txt");

                    string eulaContent = $"""
#By changing the setting below to TRUE you are indicating your agreement to our EULA (https://aka.ms/MinecraftEULA).
#{DateTime.Now:ddd MMM dd HH:mm:ss K yyyy}
eula=true
""";

                    File.WriteAllText(eulafilePath, eulaContent);
                }
            }
        }

        public void FinishOOBE()
        {
            if (DataHelper.Settings != null && DataHelper.Settings.Server.Edition == 0)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "CheckNetIsolation.exe",
                        Arguments = "LoopbackExempt -a -p=S-1-15-2-1958404141-86561845-1752920682-3514627264-368642714-62675701-733520436",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "CheckNetIsolation.exe",
                        Arguments = "LoopbackExempt -a -p=S-1-15-2-424268864-5579737-879501358-346833251-474568803-887069379-4040235476",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });
                }
                catch
                { }
            }
            OOBEFinished?.Invoke(true);
        }
    }
}
