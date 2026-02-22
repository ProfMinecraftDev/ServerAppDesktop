namespace ServerAppDesktop.Services;

public interface IOOBEService
{
    public event Action<bool>? OOBEFinished;
    public void SaveUserSettings(AppSettings appSettings);
    public void FinishOOBE();
}
