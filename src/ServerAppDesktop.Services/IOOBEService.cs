using ServerAppDesktop.Models;

namespace ServerAppDesktop.Services
{
    public interface IOOBEService
    {
        void SaveUserSettings(AppSettings appSettings);
        void RestartApplication();
    }
}
