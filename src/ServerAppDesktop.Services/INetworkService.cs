using System.Threading.Tasks;

namespace ServerAppDesktop.Services
{
    public interface INetworkService
    {
        string GetLocalIP();
        Task<string> GetPublicIPAsync();
    }
}
