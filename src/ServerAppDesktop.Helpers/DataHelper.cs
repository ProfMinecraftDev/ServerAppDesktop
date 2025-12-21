using System.Security.Principal;

namespace ServerAppDesktop.Helpers
{
    public static partial class DataHelper
    {
        public static readonly string AppName = "Server App Desktop";
        public static readonly string WindowTitle = "Server App Desktop (Preview)";
        public static readonly string WindowSubtitle = string.Empty;
        public static readonly bool DebugMode =
#if DEBUG
                true;
#else
                false;
#endif

        static DataHelper()
        {
            bool admin = false;

            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            admin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        }
    }
}
