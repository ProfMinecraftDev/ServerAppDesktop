namespace ServerAppDesktop.Services;

public interface IOOBEService
{
    public event TypedEventHandler<IOOBEService, OOBEFinishedEventArgs>? OOBEFinished;
    public void SaveUserSettings(AppSettings appSettings);
    public void FinishOOBE();
}
