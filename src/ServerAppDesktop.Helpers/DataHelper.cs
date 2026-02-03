using System;
using System.IO;
using System.Security.Principal;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Helpers
{
    public static partial class DataHelper
    {
        /// <summary>
        /// <c>Canal de actualizaciones</c>
        /// <para />
        /// <c>Canal Stable:</c> 0
        /// <para />
        /// <c>Canal Preview:</c> 1
        /// </summary>
        public static int UpdateChannel { get; } = 1;

        /// <summary>
        /// <c>Window ID para el singleton</c>
        /// <para />
        /// <c>ID para Preview Channel:</c> ServerAppDesktop_74af9644-d6d0-4762-84ab-c54826171600
        /// <para />
        /// <c>ID para Stable Channel:</c> ServerAppDesktop_3e2b8f4a-1c4e-4d3b-9f7a-2f4e5d6c7b8a
        /// <para />
        /// Cambiar según el canal de actualizaciones
        /// </summary>
        public static string WindowIdentifier
        {
            get
            {
                if (UpdateChannel == 0)
                    return "ServerAppDesktop_3e2b8f4a-1c4e-4d3b-9f7a-2f4e5d6c7b8a";
                else if (UpdateChannel == 1)
                    return "ServerAppDesktop_74af9644-d6d0-4762-84ab-c54826171600";
                else
                    throw new IndexOutOfRangeException("El canal de actualización de Server App Desktop es inválido: " + UpdateChannel);

            }
        }

        /// <summary>
        /// <c>AppUserModelID (AUMID) para la app</c>
        /// <para />
        /// <c>Stable Channel:</c> ProfMinecraftDev.ServerAppDesktop
        /// <para />
        /// <c>Preview Channel: </c> ProfMinecraftDev.ServerAppDesktop.Preview
        /// </summary>
        public static string AppUserModelID
        {
            get
            {
                if (UpdateChannel == 0)
                    return "ProfMinecraftDev.ServerAppDesktop";
                else if (UpdateChannel == 1)
                    return "ProfMinecraftDev.ServerAppDesktop.Preview";
                else
                    throw new IndexOutOfRangeException("El canal de actualización de Server App Desktop es inválido: " + UpdateChannel);

            }
        }

        public static AppSettings? Settings { get; set; } = null;
        public static string AppName { get; } = "Server App Desktop";
        public static string AppVersion { get; } = "1.0.0.3 (Preview)";
        public static string AppVersionTag { get; } = "v1.0.0.3-Preview";
        public static string WindowTitle { get; } = "Server App Desktop (Preview)";
        public static string WindowSubtitle { get; private set; } = string.Empty;
        public static string GitHubUsername { get; } = "ProfMinecraftDev";
        public static string GitHubRepository { get; } = "ServerAppDesktop";
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
