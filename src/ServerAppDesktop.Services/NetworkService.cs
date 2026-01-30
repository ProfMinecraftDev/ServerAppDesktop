using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerAppDesktop.Services;

public class NetworkService : INetworkService
{
    // Obtiene la IP de la tarjeta de red activa
    public string GetLocalIP()
    {
        // Usamos 'using' para que el socket se cierre y se limpie de la RAM automáticamente
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            try
            {
                // Cambié a UDP (Dgram) porque es más rápido y no intenta establecer un "handshake" real
                socket.Connect("8.8.8.8", 65530);

                if (socket.LocalEndPoint is IPEndPoint endPoint)
                {
                    return endPoint.Address.ToString();
                }

                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }

    // Obtiene la IP que ve el mundo (WAN) usando un servicio externo
    public async Task<string> GetPublicIPAsync()
    {
        try
        {
            using var client = new HttpClient();
            return await client.GetStringAsync("https://api.ipify.org");
        }
        catch { return "Desconocida"; }
    }
}