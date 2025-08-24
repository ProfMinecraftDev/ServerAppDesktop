using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ServerAppDesktop.Utils;


namespace ServerAppDesktop.Services
{
    /// <summary>
    /// Hechizería de red - Porque encontrar tu IP local no debería ser ciencia espacial
    /// </summary>
    public static class NetworkService
    {
        /// <summary>
        /// Obtiene la IP local más probable para el servidor (spoiler: probablemente no sea 127.0.0.1)
        /// </summary>
        public static async Task<string> GetLocalIpAddressAsync()
        {
            return await NetworkHelper.GetLocalIPAddressAsync();
        }

        /// <summary>
        /// Verifica si la IP está en el misterioso rango 172.16.x.x (territorio de red corporativa)
        /// </summary>
        private static bool IsInRange172(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out var ip))
            {
                var bytes = ip.GetAddressBytes();
                return bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31;
            }
            return false;
        }

        /// <summary>
        /// Verifica si un puerto está disponible (como verificar si hay un lugar de estacionamiento libre)
        /// </summary>
        public static bool IsPortAvailable(int port)
        {
            return NetworkHelper.IsPortAvailable(port);
        }

        /// <summary>
        /// Encuentra el siguiente puerto disponible desde el puerto dado (modo cacería de puertos activado)
        /// </summary>
        public static int FindNextAvailablePort(int startPort = 19132)
        {
            return NetworkHelper.FindAvailablePort(startPort);
        }

        /// <summary>
        /// Obtiene información detallada de las interfaces de red (la autobiografía completa de la red)
        /// </summary>
        public static List<NetworkInterfaceInfo> GetNetworkInterfaces()
        {
            var interfaces = new List<NetworkInterfaceInfo>();

            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var ni in networkInterfaces)
                {
                    var ipProperties = ni.GetIPProperties();
                    var unicastAddresses = ipProperties.UnicastAddresses
                        .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)
                        .Select(ua => ua.Address.ToString())
                        .ToList();

                    if (unicastAddresses.Any())
                    {
                        interfaces.Add(new NetworkInterfaceInfo
                        {
                            Name = ni.Name,
                            Description = ni.Description,
                            Type = ni.NetworkInterfaceType.ToString(),
                            Status = ni.OperationalStatus.ToString(),
                            IpAddresses = unicastAddresses
                        });
                    }
                }
            }
            catch
            {
                // Las interfaces de red decidieron jugar al escondite
            }

            return interfaces;
        }
    }

    /// <summary>
    /// Información de interfaz de red - Todo lo que querías saber pero tenías miedo de preguntar
    /// </summary>
    public class NetworkInterfaceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> IpAddresses { get; set; } = new List<string>();
        
        public string PrimaryIpAddress => IpAddresses.FirstOrDefault() ?? "N/A";
    }
}
