using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ServerAppDesktop.Models;
using System;

namespace ServerAppDesktop.Services
{
    public static class PropertiesFileService
    {
        /// <summary>
        /// Lee un archivo de propiedades y devuelve un diccionario con los valores
        /// </summary>
        public static async Task<Dictionary<string, string>> ReadPropertiesFileAsync(string filePath)
        {
            var properties = new Dictionary<string, string>();

            try
            {
                if (!File.Exists(filePath))
                {
                    return properties;
                }

                var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    // Ignorar líneas vacías y comentarios
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("!"))
                    {
                        continue;
                    }

                    // Buscar el separador = o :
                    var separatorIndex = FindPropertySeparator(trimmedLine);
                    if (separatorIndex == -1)
                    {
                        continue; // Línea sin separador válido
                    }

                    var key = trimmedLine.Substring(0, separatorIndex).Trim();
                    var value = trimmedLine.Substring(separatorIndex + 1).Trim();

                    // Procesar escapes básicos
                    value = ProcessEscapes(value);

                    if (!string.IsNullOrEmpty(key))
                    {
                        properties[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error leyendo archivo properties: {ex.Message}");
            }

            return properties;
        }

        /// <summary>
        /// Obtiene el puerto del servidor desde el archivo de propiedades
        /// </summary>
        public static async Task<int?> GetServerPortAsync(string propertiesFilePath)
        {
            try
            {
                var properties = await ReadPropertiesFileAsync(propertiesFilePath);

                // Buscar la propiedad del puerto según la edición
                string[] portKeys = { "server-port", "port", "bedrock-port", "minecraft-port" };

                foreach (var key in portKeys)
                {
                    if (properties.TryGetValue(key, out var portValue))
                    {
                        if (int.TryParse(portValue, out var port) && port > 0 && port <= 65535)
                        {
                            return port;
                        }
                    }
                }

                // Si no se encuentra, devolver puerto por defecto según el tipo de archivo
                var fileName = Path.GetFileName(propertiesFilePath).ToLowerInvariant();
                if (fileName.Contains("bedrock"))
                {
                    return 19132; // Puerto por defecto de Bedrock
                }
                else
                {
                    return 25565; // Puerto por defecto de Java
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene información básica del servidor desde el archivo de propiedades
        /// </summary>
        public static async Task<ServerPropertiesInfo> GetServerInfoAsync(string propertiesFilePath)
        {
            var info = new ServerPropertiesInfo();

            try
            {
                var properties = await ReadPropertiesFileAsync(propertiesFilePath);

                // Puerto del servidor
                var port = await GetServerPortAsync(propertiesFilePath);
                if (port.HasValue)
                {
                    info.Port = port.Value;
                }

                // Nombre del servidor
                if (properties.TryGetValue("server-name", out var serverName) ||
                    properties.TryGetValue("motd", out serverName) ||
                    properties.TryGetValue("level-name", out serverName))
                {
                    info.ServerName = serverName;
                }

                // Modo de juego
                if (properties.TryGetValue("gamemode", out var gamemode))
                {
                    info.GameMode = gamemode;
                }

                // Dificultad
                if (properties.TryGetValue("difficulty", out var difficulty))
                {
                    info.Difficulty = difficulty;
                }

                // Número máximo de jugadores
                if (properties.TryGetValue("max-players", out var maxPlayersStr) &&
                    int.TryParse(maxPlayersStr, out var maxPlayers))
                {
                    info.MaxPlayers = maxPlayers;
                }

                // Mundo/Nivel
                if (properties.TryGetValue("level-name", out var levelName))
                {
                    info.WorldName = levelName;
                }

                // IP del servidor (si está especificada)
                if (properties.TryGetValue("server-ip", out var serverIp) && !string.IsNullOrWhiteSpace(serverIp))
                {
                    info.ServerIp = serverIp;
                }

                info.IsValid = true;
            }
            catch (Exception ex)
            {
                info.ErrorMessage = $"Error leyendo propiedades: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(info.ErrorMessage);
            }

            return info;
        }

        /// <summary>
        /// Actualiza una propiedad específica en el archivo
        /// </summary>
        public static async Task<bool> UpdatePropertyAsync(string filePath, string key, string value)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
                var updated = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith(key + "=") || line.StartsWith(key + ":"))
                    {
                        lines[i] = $"{key}={value}";
                        updated = true;
                        break;
                    }
                }

                // Si la propiedad no existe, agregarla al final
                if (!updated)
                {
                    var newLines = lines.ToList();
                    newLines.Add($"{key}={value}");
                    lines = newLines.ToArray();
                }

                await File.WriteAllLinesAsync(filePath, lines, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encuentra el índice del separador de propiedad (= o :)
        /// </summary>
        private static int FindPropertySeparator(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '=' || ch == ':')
                {
                    return i;
                }
                // Si encontramos un espacio, puede ser un separador con espacios
                if (ch == ' ')
                {
                    // Buscar = o : después del espacio
                    for (int j = i + 1; j < line.Length; j++)
                    {
                        if (line[j] == '=' || line[j] == ':')
                        {
                            return j;
                        }
                        if (line[j] != ' ')
                        {
                            break; // No es un separador válido
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Procesa escapes básicos en los valores de las propiedades
        /// </summary>
        private static string ProcessEscapes(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\\", "\\");
        }

        /// <summary>
        /// Detecta si un archivo es de Bedrock o Java Edition basado en su contenido
        /// </summary>
        public static async Task<string> DetectEditionAsync(string propertiesFilePath)
        {
            try
            {
                var properties = await ReadPropertiesFileAsync(propertiesFilePath);

                // Propiedades típicas de Bedrock Edition
                string[] bedrockProperties = {
                    "server-name", "allow-cheats", "default-player-permission-level",
                    "texturepack-required", "content-log-file-enabled", "compression-threshold"
                };

                // Propiedades típicas de Java Edition
                string[] javaProperties = {
                    "enable-jmx-monitoring", "rcon.port", "level-seed", "gamemode",
                    "enable-command-block", "enable-query", "generator-settings"
                };

                int bedrockScore = bedrockProperties.Count(prop => properties.ContainsKey(prop));
                int javaScore = javaProperties.Count(prop => properties.ContainsKey(prop));

                if (bedrockScore > javaScore)
                {
                    return "Bedrock";
                }
                else if (javaScore > bedrockScore)
                {
                    return "Java";
                }
                else
                {
                    // No se puede determinar, usar heurística basada en el puerto por defecto
                    var port = await GetServerPortAsync(propertiesFilePath);
                    return port == 19132 ? "Bedrock" : "Java";
                }
            }
            catch
            {
                return "Bedrock"; // Por defecto
            }
        }
    }

    /// <summary>
    /// Información extraída del archivo de propiedades del servidor
    /// </summary>
    public class ServerPropertiesInfo
    {
        public string ServerName { get; set; } = "Servidor de Minecraft";
        public int Port { get; set; } = 19132;
        public string ServerIp { get; set; } = string.Empty;
        public string GameMode { get; set; } = "survival";
        public string Difficulty { get; set; } = "easy";
        public int MaxPlayers { get; set; } = 10;
        public string WorldName { get; set; } = "Bedrock level";
        public bool IsValid { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
