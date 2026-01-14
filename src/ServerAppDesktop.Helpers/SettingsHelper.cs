using System.IO;

namespace ServerAppDesktop.Helpers
{
    public sealed class SettingsHelper
    {
        public static bool ExistsConfigurationFile() =>
            File.Exists(Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile));
    }
}
