using System.Security.Principal;

namespace ServerAppDesktop.Helpers
{
    public static partial class DataHelper
    {
        public static string AppName { get; } = "Server App Desktop";
        public static string WindowTitle { get; } = "Server App Desktop (Preview)";
        public static string WindowSubtitle { get; } = string.Empty;
        public static bool RunAsAdmin { get; } = false;
        public static bool DebugMode { get; } =
#if DEBUG
                true;
#else
                false;
#endif

        static DataHelper()
        {
            RunAsAdmin = false;

            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            RunAsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (DebugMode)
                WindowSubtitle = ResourceHelper.GetString("DebugModeIndicator");
            else if (RunAsAdmin)
                WindowSubtitle = ResourceHelper.GetString("AdminModeIndicator");
            else if (RunAsAdmin && DebugMode)
                WindowSubtitle = ResourceHelper.GetString("AdminDebugModeIndicator");
        }
    }
}
