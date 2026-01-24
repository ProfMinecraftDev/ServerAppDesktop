using System;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Services
{
    public interface IOOBEService
    {
        event Action<bool>? OOBEFinished;
        void SaveUserSettings(AppSettings appSettings);
        void FinishOOBE();
    }
}
