using System.Security.Principal;
using System.Threading;

namespace ServerAppDesktop.Helpers
{
    public static partial class DataHelper
    {
        public static string MutexIdentifier { get; } = "ServerAppDesktop_Unique_Mutex_Identifier_123456789";
        public static string AppName { get; } = "Server App Desktop";
        public static string WindowTitle { get; } = "Server App Desktop (Preview)";
        public static string WindowSubtitle { get; private set; } = string.Empty;

        public static bool RunAsAdmin { get; private set; }
        public static bool DebugMode { get; } =
#if DEBUG
            true;
#else
            false;
#endif

        public static Mutex? AppMutex { get; set; }

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
