using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerAppDesktop.Utils
{
    /// <summary>
    /// Utilidad para operaciones de red y detección automática
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// Obtiene la IP local preferida del sistema (IPv4, no loopback)
        /// </summary>
        public static async Task<string> GetLocalIPAddressAsync()
        {
            try
            {
                // Método 1: Intentar conectar a un servidor externo para obtener la IP local usada
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    await socket.ConnectAsync("8.8.8.8", 80);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null)
                        return endPoint.Address.ToString();
                }
            }
            catch
            {
                // Si falla, usar método alternativo
            }

            // Método 2: Obtener interfaces de red activas
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .OrderBy(ni => GetInterfacePriority(ni.NetworkInterfaceType));

                foreach (var networkInterface in networkInterfaces)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    var ipv4Address = ipProperties.UnicastAddresses
                        .FirstOrDefault(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork &&
                                             !IPAddress.IsLoopback(ua.Address) &&
                                             !IsLinkLocal(ua.Address));

                    if (ipv4Address != null)
                        return ipv4Address.Address.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo IP local: {ex.Message}");
            }

            // Método 3: Fallback usando Dns.GetHostEntry
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());
                var ipv4Address = hostEntry.AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork &&
                                         !IPAddress.IsLoopback(ip) &&
                                         !IsLinkLocal(ip));

                if (ipv4Address != null)
                    return ipv4Address.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error usando DNS fallback: {ex.Message}");
            }

            // Último recurso
            return "127.0.0.1";
        }

        /// <summary>
        /// Obtiene la IP local de forma síncrona
        /// </summary>
        public static string GetLocalIPAddress()
        {
            return GetLocalIPAddressAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Extrae el puerto del servidor desde un archivo server.properties
        /// </summary>
        public static async Task<int?> GetServerPortFromPropertiesAsync(string propertiesFilePath)
        {
            if (!File.Exists(propertiesFilePath))
                return null;

            try
            {
                var lines = await File.ReadAllLinesAsync(propertiesFilePath);
                
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    // Saltar comentarios y líneas vacías
                    if (trimmedLine.StartsWith("#") || string.IsNullOrWhiteSpace(trimmedLine))
                        continue;

                    // Buscar server-port
                    if (trimmedLine.StartsWith("server-port=", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = trimmedLine.Substring("server-port=".Length).Trim();
                        if (int.TryParse(value, out int port) && port > 0 && port <= 65535)
                            return port;
                    }

                    // Para servidores Java, también buscar query.port como alternativa
                    if (trimmedLine.StartsWith("query.port=", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = trimmedLine.Substring("query.port=".Length).Trim();
                        if (int.TryParse(value, out int queryPort) && queryPort > 0 && queryPort <= 65535)
                            return queryPort;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error leyendo archivo properties: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Verifica si un puerto está disponible en el sistema
        /// </summary>
        public static bool IsPortAvailable(int port)
        {
            try
            {
                // Verificar TCP
                var tcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
                if (tcpListeners.Any(listener => listener.Port == port))
                    return false;

                // Verificar UDP
                var udpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
                if (udpListeners.Any(listener => listener.Port == port))
                    return false;

                // Intentar bind temporal
                using (var tcpListener = new TcpListener(IPAddress.Any, port))
                {
                    tcpListener.Start();
                    tcpListener.Stop();
                }

                using (var udpClient = new UdpClient(port))
                {
                    udpClient.Close();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encuentra el siguiente puerto disponible a partir de un puerto base
        /// </summary>
        public static int FindAvailablePort(int startPort = 19132, int maxAttempts = 100)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                int port = startPort + i;
                if (port > 65535) break;
                
                if (IsPortAvailable(port))
                    return port;
            }

            throw new InvalidOperationException($"No se encontró un puerto disponible después de {maxAttempts} intentos desde el puerto {startPort}");
        }

        /// <summary>
        /// Verifica si se puede conectar a un host y puerto específico
        /// </summary>
        public static async Task<bool> CanConnectAsync(string host, int port, int timeoutMs = 5000)
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    var connectTask = tcpClient.ConnectAsync(host, port);
                    var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutMs));
                    
                    if (completedTask == connectTask && tcpClient.Connected)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Conexión falló
            }

            return false;
        }

        /// <summary>
        /// Obtiene información detallada de la red local
        /// </summary>
        public static async Task<NetworkInfo> GetNetworkInfoAsync()
        {
            var info = new NetworkInfo();

            try
            {
                info.LocalIP = await GetLocalIPAddressAsync();
                info.HostName = Dns.GetHostName();

                // Obtener adaptadores de red activos
                var activeInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .ToList();

                info.ActiveNetworkAdapters = activeInterfaces.Count;
                info.HasWiredConnection = activeInterfaces.Any(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
                info.HasWirelessConnection = activeInterfaces.Any(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);

                // Verificar conectividad a Internet
                info.HasInternetConnection = await CanConnectAsync("8.8.8.8", 53, 3000);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo información de red: {ex.Message}");
            }

            return info;
        }

        private static int GetInterfacePriority(NetworkInterfaceType type)
        {
            // Priorizar interfaces Ethernet sobre WiFi
            return type switch
            {
                NetworkInterfaceType.Ethernet => 1,
                NetworkInterfaceType.GigabitEthernet => 1,
                NetworkInterfaceType.FastEthernetT => 2,
                NetworkInterfaceType.Wireless80211 => 3,
                _ => 10
            };
        }

        private static bool IsLinkLocal(IPAddress address)
        {
            // Direcciones link-local (169.254.x.x)
            var bytes = address.GetAddressBytes();
            return bytes[0] == 169 && bytes[1] == 254;
        }
    }

    /// <summary>
    /// Información detallada de la red local
    /// </summary>
    public class NetworkInfo
    {
        public string LocalIP { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public int ActiveNetworkAdapters { get; set; }
        public bool HasWiredConnection { get; set; }
        public bool HasWirelessConnection { get; set; }
        public bool HasInternetConnection { get; set; }
    }
}
