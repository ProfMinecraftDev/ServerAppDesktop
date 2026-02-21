
namespace ServerAppDesktop.Services;

public class NetworkService : INetworkService
{

    public string GetLocalIP()
    {

        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try
        {

            socket.Connect("8.8.8.8", 65530);

            return socket.LocalEndPoint is IPEndPoint endPoint ? endPoint.Address.ToString() : "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }


    public async Task<string> GetPublicIPAsync()
    {
        try
        {
            using HttpClient client = new();
            return await client.GetStringAsync("https://api.ipify.org/");
        }
        catch { return "Desconocida"; }
    }
}
