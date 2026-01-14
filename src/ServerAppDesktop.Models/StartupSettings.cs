namespace ServerAppDesktop.Models
{
    public sealed class StartupSettings
    {
        public bool AutoStartServer { get; set; } = false;
        public bool CloseInSystemTray { get; set; } = false;
    }
}
