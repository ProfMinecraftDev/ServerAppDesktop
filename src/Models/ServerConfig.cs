using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ServerAppDesktop.Models
{
    public enum ServerType
    {
        Bedrock,
        Java
    }

    public class ServerConfig : INotifyPropertyChanged
    {
        private ServerType _serverType = ServerType.Bedrock;
        private string _serverPath = string.Empty;
        private string _executablePath = string.Empty;
        private string _serverIp = string.Empty;
        private int _serverPort = 19132; // Puerto por defecto de Bedrock
        private string _startCommand = string.Empty;
        private bool _autoStart = false;
        private bool _useAutoIp = true;
        private bool _useAutoPort = true;
        private string _theme = "Sistema";
        private string _backdrop = "Mica Alt";

        /// <summary>
        /// Tipo de servidor (Bedrock/Java)
        /// </summary>
        public ServerType ServerType
        {
            get => _serverType;
            set
            {
                if (_serverType != value)
                {
                    _serverType = value;
                    // Cambiar puerto por defecto según el tipo
                    if (value == ServerType.Bedrock && ServerPort == 25565)
                        ServerPort = 19132;
                    else if (value == ServerType.Java && ServerPort == 19132)
                        ServerPort = 25565;
                    
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Ruta completa al directorio del servidor
        /// </summary>
        public string ServerPath
        {
            get => _serverPath;
            set => SetProperty(ref _serverPath, value ?? string.Empty);
        }

        /// <summary>
        /// Ruta completa al ejecutable del servidor
        /// </summary>
        public string? ExecutablePath
        {
            get => _executablePath;
            set => SetProperty(ref _executablePath, value ?? string.Empty);
        }

        /// <summary>
        /// IP del servidor
        /// </summary>
        public string ServerIp
        {
            get => _serverIp;
            set => SetProperty(ref _serverIp, value ?? string.Empty);
        }

        /// <summary>
        /// Puerto del servidor
        /// </summary>
        public int ServerPort
        {
            get => _serverPort;
            set
            {
                // Validar rango de puerto
                if (value < 1 || value > 65535)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "El puerto debe estar entre 1 y 65535");
                }
                SetProperty(ref _serverPort, value);
            }
        }

        /// <summary>
        /// Comando personalizado para iniciar el servidor
        /// </summary>
        public string StartCommand
        {
            get => _startCommand;
            set => SetProperty(ref _startCommand, value ?? string.Empty);
        }

        /// <summary>
        /// Indica si el servidor debe iniciarse automáticamente
        /// </summary>
        public bool AutoStart
        {
            get => _autoStart;
            set => SetProperty(ref _autoStart, value);
        }

        /// <summary>
        /// Usar detección automática de IP
        /// </summary>
        public bool UseAutoIp
        {
            get => _useAutoIp;
            set => SetProperty(ref _useAutoIp, value);
        }

        /// <summary>
        /// Usar puerto automático desde archivo .properties
        /// </summary>
        public bool UseAutoPort
        {
            get => _useAutoPort;
            set => SetProperty(ref _useAutoPort, value);
        }

        /// <summary>
        /// Tema de la aplicación
        /// </summary>
        public string Theme
        {
            get => _theme;
            set
            {
                var validThemes = new[] { "Sistema", "Claro", "Oscuro" };
                var themeValue = value ?? "Sistema";
                if (!validThemes.Contains(themeValue))
                {
                    themeValue = "Sistema";
                }
                SetProperty(ref _theme, themeValue);
            }
        }

        /// <summary>
        /// Backdrop de la ventana
        /// </summary>
        public string Backdrop
        {
            get => _backdrop;
            set
            {
                var validBackdrops = new[] { "Mica", "Mica Alt", "Desktop Acrylic" };
                var backdropValue = value ?? "Mica Alt";
                if (!validBackdrops.Contains(backdropValue))
                {
                    backdropValue = "Mica Alt";
                }
                SetProperty(ref _backdrop, backdropValue);
            }
        }

        /// <summary>
        /// Indica si la configuración está completa y es válida
        /// </summary>
        public bool IsValid
        {
            get
            {
                try
                {
                    return !string.IsNullOrWhiteSpace(ServerPath) && 
                           !string.IsNullOrWhiteSpace(ExecutablePath) && 
                           File.Exists(ExecutablePath) &&
                           Directory.Exists(ServerPath) &&
                           !string.IsNullOrWhiteSpace(ServerIp) &&
                           ServerPort > 0 && ServerPort <= 65535;
                }
                catch
                {
                    return false; // Si hay error accediendo al sistema de archivos, configuración inválida
                }
            }
        }

        /// <summary>
        /// Obtiene la ruta del archivo de propiedades del servidor
        /// </summary>
        public string PropertiesFilePath
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(ServerPath))
                        return string.Empty;
                        
                    return ServerType == ServerType.Bedrock 
                        ? Path.Combine(ServerPath, "server.properties")
                        : Path.Combine(ServerPath, "server.properties");
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
