namespace ServerAppDesktop.Services;

public interface INetworkService
{
    public string GetLocalIP();
    public Task<string> GetPublicIPAsync();
}
