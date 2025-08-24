using ServerAppDesktop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ServerAppDesktop.Utils
{
    /// <summary>
    /// Resultado de la validación de configuración
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        
        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }

        public void AddError(string error) => Errors.Add(error);
        public void AddWarning(string warning) => Warnings.Add(warning);
    }

    /// <summary>
    /// Validador robusto para configuraciones del servidor
    /// </summary>
    public static class ConfigValidator
    {
        private static readonly Regex IpV4Regex = new Regex(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Valida completamente una configuración de servidor
        /// </summary>
        public static ValidationResult Validate(ServerConfig config)
        {
            if (config == null)
                return ValidationResult.Failure("La configuración no puede ser nula");

            var result = new ValidationResult { IsValid = true };

            // Validar tipo de servidor
            ValidateServerType(config, result);

            // Validar rutas
            ValidateServerPath(config, result);
            ValidateExecutablePath(config, result);

            // Validar configuración de red
            ValidateNetworkConfig(config, result);

            // Validar configuración avanzada
            ValidateAdvancedConfig(config, result);

            // Si hay errores, marcar como inválido
            result.IsValid = result.Errors.Count == 0;

            return result;
        }

        /// <summary>
        /// Valida solo los campos esenciales para operación básica
        /// </summary>
        public static ValidationResult ValidateEssentials(ServerConfig config)
        {
            if (config == null)
                return ValidationResult.Failure("La configuración no puede ser nula");

            var result = new ValidationResult { IsValid = true };

            // Solo validaciones críticas
            if (string.IsNullOrWhiteSpace(config.ServerPath))
                result.AddError("La ruta del servidor es obligatoria");

            if (string.IsNullOrWhiteSpace(config.ExecutablePath))
                result.AddError("La ruta del ejecutable es obligatoria");

            if (string.IsNullOrWhiteSpace(config.ServerIp))
                result.AddError("La IP del servidor es obligatoria");

            if (config.ServerPort <= 0 || config.ServerPort > 65535)
                result.AddError("El puerto debe estar entre 1 y 65535");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Valida si una configuración es suficiente para ejecutar el servidor
        /// </summary>
        public static ValidationResult ValidateForExecution(ServerConfig config)
        {
            var result = Validate(config);

            if (result.IsValid)
            {
                // Validaciones adicionales para ejecución
                if (!Directory.Exists(config.ServerPath))
                    result.AddError($"El directorio del servidor no existe: {config.ServerPath}");

                if (!File.Exists(config.ExecutablePath))
                    result.AddError($"El ejecutable del servidor no existe: {config.ExecutablePath}");

                // Verificar permisos de escritura en el directorio
                try
                {
                    var testFile = Path.Combine(config.ServerPath, $"test_write_{Guid.NewGuid():N}.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch
                {
                    result.AddWarning("El directorio del servidor podría no tener permisos de escritura");
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        private static void ValidateServerType(ServerConfig config, ValidationResult result)
        {
            // ServerType es enum, siempre válido, pero podemos validar consistencia
            if (config.ServerType == ServerType.Bedrock && config.ServerPort == 25565)
                result.AddWarning("Puerto Java (25565) configurado para servidor Bedrock");
            
            if (config.ServerType == ServerType.Java && config.ServerPort == 19132)
                result.AddWarning("Puerto Bedrock (19132) configurado para servidor Java");
        }

        private static void ValidateServerPath(ServerConfig config, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.ServerPath))
            {
                result.AddError("La ruta del servidor es obligatoria");
                return;
            }

            // Validar formato de ruta
            try
            {
                var fullPath = Path.GetFullPath(config.ServerPath);
                if (config.ServerPath != fullPath)
                    result.AddWarning($"Se recomienda usar rutas absolutas: {fullPath}");
            }
            catch (Exception)
            {
                result.AddError($"Formato de ruta inválido: {config.ServerPath}");
                return;
            }

            // Verificar si existe
            if (!Directory.Exists(config.ServerPath))
                result.AddWarning($"El directorio no existe: {config.ServerPath}");

            // Verificar que no sea directorio del sistema
            var systemPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            if (systemPaths.Any(sysPath => config.ServerPath.StartsWith(sysPath, StringComparison.OrdinalIgnoreCase)))
                result.AddWarning("No se recomienda usar directorios del sistema para el servidor");
        }

        private static void ValidateExecutablePath(ServerConfig config, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.ExecutablePath))
            {
                result.AddError("La ruta del ejecutable es obligatoria");
                return;
            }

            // Validar formato
            try
            {
                Path.GetFullPath(config.ExecutablePath);
            }
            catch (Exception)
            {
                result.AddError($"Formato de ruta del ejecutable inválido: {config.ExecutablePath}");
                return;
            }

            // Validar extensión según tipo de servidor
            var extension = Path.GetExtension(config.ExecutablePath).ToLowerInvariant();
            
            if (config.ServerType == ServerType.Java && extension != ".jar")
                result.AddWarning("Los servidores Java generalmente usan archivos .jar");
            
            if (config.ServerType == ServerType.Bedrock && extension != ".exe")
                result.AddWarning("Los servidores Bedrock generalmente usan archivos .exe");

            // Verificar si existe
            if (!File.Exists(config.ExecutablePath))
                result.AddWarning($"El archivo ejecutable no existe: {config.ExecutablePath}");
        }

        private static void ValidateNetworkConfig(ServerConfig config, ValidationResult result)
        {
            // Validar IP
            if (string.IsNullOrWhiteSpace(config.ServerIp))
            {
                result.AddError("La IP del servidor es obligatoria");
            }
            else if (!IsValidIpAddress(config.ServerIp))
            {
                result.AddError($"Formato de IP inválido: {config.ServerIp}");
            }
            else if (config.ServerIp == "127.0.0.1" || config.ServerIp == "localhost")
            {
                result.AddWarning("IP localhost solo permitirá conexiones locales");
            }

            // Validar puerto
            if (config.ServerPort <= 0 || config.ServerPort > 65535)
            {
                result.AddError($"Puerto inválido: {config.ServerPort}. Debe estar entre 1 y 65535");
            }
            else if (config.ServerPort < 1024)
            {
                result.AddWarning($"Puerto {config.ServerPort} es un puerto del sistema, puede requerir permisos administrativos");
            }

            // Puertos comunes que podrían estar en uso
            var commonPorts = new[] { 80, 443, 21, 22, 23, 25, 53, 110, 993, 995 };
            if (commonPorts.Contains(config.ServerPort))
                result.AddWarning($"El puerto {config.ServerPort} es comúnmente usado por otros servicios");
        }

        private static void ValidateAdvancedConfig(ServerConfig config, ValidationResult result)
        {
            // Validar tema
            if (string.IsNullOrWhiteSpace(config.Theme))
                config.Theme = "Sistema"; // Valor por defecto

            var validThemes = new[] { "Sistema", "Light", "Dark" };
            if (!validThemes.Contains(config.Theme))
                result.AddWarning($"Tema desconocido: {config.Theme}. Valores válidos: {string.Join(", ", validThemes)}");

            // Validar backdrop
            if (string.IsNullOrWhiteSpace(config.Backdrop))
                config.Backdrop = "Mica Alt"; // Valor por defecto

            var validBackdrops = new[] { "Mica Alt", "Mica", "Desktop Acrylic", "Ninguno" };
            if (!validBackdrops.Contains(config.Backdrop))
                result.AddWarning($"Backdrop desconocido: {config.Backdrop}. Valores válidos: {string.Join(", ", validBackdrops)}");

            // Validar comando de inicio personalizado
            if (!string.IsNullOrWhiteSpace(config.StartCommand))
            {
                if (config.StartCommand.Length > 1000)
                    result.AddWarning("Comando de inicio muy largo, podría causar problemas");

                // Caracteres peligrosos básicos
                if (config.StartCommand.Contains("&&") || config.StartCommand.Contains("||") || config.StartCommand.Contains("|"))
                    result.AddWarning("Comando contiene operadores de shell, verificar seguridad");
            }
        }

        private static bool IsValidIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            // Verificar con regex básico
            if (!IpV4Regex.IsMatch(ip))
                return false;

            // Verificar con IPAddress.TryParse para mayor precisión
            return IPAddress.TryParse(ip, out var address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }
    }
}
