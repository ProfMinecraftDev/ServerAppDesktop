namespace ServerAppDesktop.Services
{
    public interface IServerPropertiesService
    {
        void SetPath(string serverPath);
        void LoadFile();
        T? GetValue<T>(string key);
        object? GetValue(string key);
        void SetValue(string key, object value);
    }
}
