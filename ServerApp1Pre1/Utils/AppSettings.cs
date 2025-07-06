// Description: Clase para manejar la configuración de la aplicación
using System.Text.Json.Serialization;

namespace ServerApp1Pre1.Utils
{
    public class AppSettings
    {
        [JsonPropertyName("serverIp")]
        public string? ServerIp { get; set; } // Dirección IP del servidor

        [JsonPropertyName("serverPort")]
        public int? ServerPort { get; set; } // Puerto del servidor

        [JsonPropertyName("serverLocation")]
        public string? ServerLocation { get; set; } // Carpeta donde está el servidor

        [JsonPropertyName("serverExeFile")]
        public string? ServerExeFile { get; set; } // Ruta al ejecutable del servidor
    }
}
