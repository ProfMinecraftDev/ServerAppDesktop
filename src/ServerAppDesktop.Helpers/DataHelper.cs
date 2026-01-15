using System;
using System.IO;
using System.Security.Principal;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Helpers
{
    public static partial class DataHelper
    {
        public static AppSettings Settings { get; set; } = new();
        public static string WindowIdentifier { get; } = "ServerAppDesktop_74af9644-d6d0-4762-84ab-c54826171600";
        public static string AppName { get; } = "Server App Desktop";
        public static string AppVersion { get; } = "1.0.0.3 (Preview)";
        public static string AppVersionTag { get; } = "v1.0.0.3-Preview";
        public static string WindowTitle { get; } = "Server App Desktop (Preview)";
        public static string WindowSubtitle { get; private set; } = string.Empty;
        public static string GitHubUsername { get; } = "ProfMinecraftDev";
        public static string GitHubRepository { get; } = "ServerAppDesktop";
        public static bool UpdateChannel { get; } = true; // false = release stable, true = preview
        public static string SettingsPath
        {
            get => Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                    ),
                "Server App Desktop (Preview)",
                "Settings"
                );
        }
        public static string SettingsFile { get; } = "appsettings.json";

        public static bool RunAsAdmin { get; private set; }
        public static bool DebugMode { get; } =
#if DEBUG
            true;
#else
            false;
#endif

        static DataHelper()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            RunAsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (RunAsAdmin && DebugMode)
                WindowSubtitle = ResourceHelper.GetString("AdminDebugModeIndicator");
            else if (DebugMode)
                WindowSubtitle = ResourceHelper.GetString("DebugModeIndicator");
            else if (RunAsAdmin)
                WindowSubtitle = ResourceHelper.GetString("AdminModeIndicator");
        }
    }
}
