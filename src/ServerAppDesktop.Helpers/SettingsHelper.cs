using System.IO;
using System.Text.Json;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using ServerAppDesktop.Models;
using Windows.Globalization;

namespace ServerAppDesktop.Helpers
{
    public sealed class SettingsHelper
    {
        public static bool ExistsConfigurationFile() =>
            File.Exists(Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile));

        public static void LoadAndSetSettings(Window window)
        {
            string fullPath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);

            if (!File.Exists(fullPath))
            {
                DataHelper.Settings = null; // O inicializa uno nuevo aquí
                return;
            }

            try
            {
                string jsonString = File.ReadAllText(fullPath);

                // 1. Validar si el archivo está vacío o solo tiene espacios
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    DataHelper.Settings = null;
                    return;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var context = new AppSettingsJsonContext(options);

                // 2. Deserializar
                var result = JsonSerializer.Deserialize(jsonString, context.AppSettings);

                // 3. Validar si el objeto es nulo o si tiene valores "fantasma" (como un {} vacío)
                // Aquí chequeas una propiedad obligatoria de tu clase AppSettings, por ejemplo 'RamLimit'
                if (result != null)
                {
                    // Usamos IsNullOrWhiteSpace para cubrir "", null y "   "
                    bool isInvalid = string.IsNullOrWhiteSpace(result.Server.Path) ||
                                     string.IsNullOrWhiteSpace(result.Server.Executable);

                    if (isInvalid)
                    {
                        DataHelper.Settings = null;
                    }
                    else
                    {
                        DataHelper.Settings = result;
                    }
                }
                else
                {
                    DataHelper.Settings = null;
                }
            }
            catch
            {
                DataHelper.Settings = null;
            }

            var settings = DataHelper.Settings;
            if (settings == null)
                return;

            window.SystemBackdrop = settings.UI.Backdrop switch
            {
                0 => new MicaBackdrop { Kind = MicaKind.Base },
                1 => new MicaBackdrop { Kind = MicaKind.BaseAlt },
                2 => new DesktopAcrylicBackdrop(),
                _ => new MicaBackdrop { Kind = MicaKind.Base }
            };

            WindowHelper.SetTheme(window, (ElementTheme)settings.UI.Theme);

            if (!string.IsNullOrEmpty(settings.UI.Language))
                ApplicationLanguages.PrimaryLanguageOverride = settings.UI.Language;
        }

        public static void SaveSettings()
        {
            if (DataHelper.Settings == null)
                return;
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
    }
}
