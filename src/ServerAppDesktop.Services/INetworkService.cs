namespace ServerAppDesktop.Services
{
    public interface INetworkService
    {
        string GetLocalIP();
        Task<string> GetPublicIPAsync();
    }
}
