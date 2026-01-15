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
            if (!ExistsConfigurationFile())
                return;

            string fullPath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);

            // 2. Verificar que el archivo existe para evitar excepciones
            if (File.Exists(fullPath))
            {
                string jsonString = File.ReadAllText(fullPath);
                var context = new AppSettingsJsonContext(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                DataHelper.Settings = JsonSerializer.Deserialize(jsonString, context.AppSettings);
            }
            else
            {
                // Si no existe, inicializar con valores por defecto
                DataHelper.Settings = new AppSettings();
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
    }
}
