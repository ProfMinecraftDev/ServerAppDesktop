namespace ServerAppDesktop.Services;

public interface ISettingsService
{
    public void Save();
    public bool GetStartWithWindows();
    public void SetStartWithWindows(bool enable);
    public int GetLanguageIndex();
}
