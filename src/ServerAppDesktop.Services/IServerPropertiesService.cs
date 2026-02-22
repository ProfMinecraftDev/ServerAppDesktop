namespace ServerAppDesktop.Services;

public interface IServerPropertiesService
{
    public void SetPath(string serverPath);
    public void LoadFile();
    public T? GetValue<T>(string key);
    public object? GetValue(string key);
    public void SetValue(string key, object value);
}
