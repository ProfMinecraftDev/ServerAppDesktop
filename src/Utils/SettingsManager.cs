using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Utils
{
    public static class SettingsManager
    {
        private static readonly string SettingsDirectory = Path.Combine(AppContext.BaseDirectory, "Settings");
        private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "Settings.json");
        private static readonly string BackupPath = Path.Combine(SettingsDirectory, "Settings.bak");
        
        private static readonly object _lock = new object();
        private static ServerConfig? _serverConfig;
        private static FileSystemWatcher? _fileWatcher;
        private static Timer? _saveTimer;
        private static bool _hasUnsavedChanges = false;
        private static bool _isInitialized = false;

        /// <summary>
        /// Inicializa el SettingsManager con auto-guardado y monitoreo de archivos
        /// </summary>
        public static Task InitializeAsync()
        {
            if (_isInitialized) return Task.CompletedTask;

            lock (_lock)
            {
                if (_isInitialized) return Task.CompletedTask;

                try
                {
                    EnsureDirectoryExists();
                    LoadConfigFromFile();
                    SetupFileWatcher();
                    SetupAutoSave();
                    _isInitialized = true;
                    
                    System.Diagnostics.Debug.WriteLine("SettingsManager inicializado correctamente");
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error inicializando SettingsManager: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Obtiene la configuración actual del servidor (thread-safe)
        /// </summary>
        public static async Task<ServerConfig> GetServerConfigAsync()
        {
            if (!_isInitialized)
                await InitializeAsync();

            lock (_lock)
            {
                if (_serverConfig == null)
                {
                    LoadConfigFromFile();
                }
                return _serverConfig ?? new ServerConfig();
            }
        }

        /// <summary>
        /// Suscribe un objeto ServerConfig para auto-guardado
        /// </summary>
        public static void Subscribe(ServerConfig config)
        {
            if (config == null) return;

            // Desuscribir configuración anterior
            if (_serverConfig != null)
            {
                _serverConfig.PropertyChanged -= OnConfigPropertyChanged;
            }

            _serverConfig = config;
            _serverConfig.PropertyChanged += OnConfigPropertyChanged;
            
            System.Diagnostics.Debug.WriteLine("ServerConfig suscrito para auto-guardado");
        }

        /// <summary>
        /// Guarda la configuración con validación
        /// </summary>
        public static async Task<bool> SaveServerConfigAsync(ServerConfig config, bool validateBeforeSave = true)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (validateBeforeSave)
            {
                var validation = ConfigValidator.Validate(config);
                if (!validation.IsValid)
                {
                    var errors = string.Join("\n", validation.Errors);
                    System.Diagnostics.Debug.WriteLine($"Validación falló: {errors}");
                    return false;
                }
            }

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        CreateBackup();
                        WriteConfigToFile(config);
                        _serverConfig = config;
                        _hasUnsavedChanges = false;
                        
                        System.Diagnostics.Debug.WriteLine("Configuración guardada exitosamente");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error guardando configuración: {ex.Message}");
                        RestoreFromBackup();
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Detecta y aplica configuración automática de red
        /// </summary>
        public static async Task<ServerConfig> ApplyAutoNetworkConfigAsync(ServerConfig config)
        {
            if (config == null) return new ServerConfig();

            var updatedConfig = new ServerConfig
            {
                ServerType = config.ServerType,
                ServerPath = config.ServerPath,
                ExecutablePath = config.ExecutablePath,
                StartCommand = config.StartCommand,
                AutoStart = config.AutoStart,
                UseAutoIp = config.UseAutoIp,
                UseAutoPort = config.UseAutoPort,
                Theme = config.Theme,
                Backdrop = config.Backdrop,
                ServerIp = config.ServerIp,
                ServerPort = config.ServerPort
            };

            try
            {
                // Auto-detectar IP si está habilitado
                if (config.UseAutoIp)
                {
                    updatedConfig.ServerIp = await NetworkHelper.GetLocalIPAddressAsync();
                    System.Diagnostics.Debug.WriteLine($"IP auto-detectada: {updatedConfig.ServerIp}");
                }

                // Auto-detectar puerto si está habilitado
                if (config.UseAutoPort && !string.IsNullOrWhiteSpace(config.ServerPath))
                {
                    var propertiesFile = Path.Combine(config.ServerPath, "server.properties");
                    var detectedPort = await NetworkHelper.GetServerPortFromPropertiesAsync(propertiesFile);
                    
                    if (detectedPort.HasValue)
                    {
                        updatedConfig.ServerPort = detectedPort.Value;
                        System.Diagnostics.Debug.WriteLine($"Puerto auto-detectado: {updatedConfig.ServerPort}");
                    }
                    else
                    {
                        // Usar puerto por defecto según tipo de servidor
                        updatedConfig.ServerPort = config.ServerType == ServerType.Java ? 25565 : 19132;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en auto-detección de red: {ex.Message}");
            }

            return updatedConfig;
        }

        /// <summary>
        /// Obtiene la configuración como objeto ServerConfig (versión síncrona)
        /// </summary>
        public static ServerConfig GetServerConfig()
        {
            if (_serverConfig != null)
                return _serverConfig;

            // Usar el método síncrono sin obsolescencia
            LoadConfigFromFile();
            return _serverConfig ?? new ServerConfig();
        }

        /// <summary>
        /// Guarda la configuración desde un objeto ServerConfig (versión síncrona)
        /// </summary>
        public static void SaveServerConfig(ServerConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
                
            lock (_lock)
            {
                try
                {
                    CreateBackup();
                    WriteConfigToFile(config);
                    _serverConfig = config;
                    _hasUnsavedChanges = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error guardando configuración: {ex.Message}");
                    RestoreFromBackup();
                }
            }
        }

        /// <summary>
        /// Carga la configuración de forma asíncrona
        /// </summary>
        public static async Task<ServerConfig?> LoadServerConfigAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    return GetServerConfig();
                }
                catch
                {
                    return null;
                }
            });
        }

        /// <summary>
        /// Guarda la configuración de forma asíncrona (sin sobrecargar SaveServerConfigAsync existente)
        /// </summary>
        public static async Task SaveServerConfigSimpleAsync(ServerConfig config)
        {
            await Task.Run(() =>
            {
                SaveServerConfig(config);
            });
        }

        private static JObject CreateEmptySettings()
        {
            var defaultConfig = new ServerConfig();
            return ConvertFromServerConfig(defaultConfig);
        }

        private static bool HasInvalidValues(JObject config)
        {
            return string.IsNullOrWhiteSpace((string?)config["edition"])
                || string.IsNullOrWhiteSpace((string?)config["serverip"])
                || (int?)config["serverport"] == 0
                || string.IsNullOrWhiteSpace((string?)config["serverlocation"])
                || string.IsNullOrWhiteSpace((string?)config["startservercommand"])
                || string.IsNullOrWhiteSpace((string?)config["theme"])
                || string.IsNullOrWhiteSpace((string?)config["mode"]);
        }

        private static void OpenInEditor()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SettingsPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowError($"No se pudo abrir el editor: {ex.Message}");
            }
        }

        private static void ShowError(string message)
        {
            DialogManager.Show(message, "Error", DialogManager.MB_OK | DialogManager.MB_ICONERROR);
        }

        /// <summary>
        /// Convierte un JObject a ServerConfig
        /// </summary>
        private static ServerConfig ConvertToServerConfig(JObject settings)
        {
            var config = new ServerConfig();

            try
            {
                var editionStr = (string?)settings["edition"] ?? "Bedrock";
                config.ServerType = editionStr == "Java" ? ServerType.Java : ServerType.Bedrock;
                config.ServerPath = (string?)settings["serverlocation"] ?? "";
                config.ExecutablePath = (string?)settings["serverexecutable"] ?? "";
                config.ServerIp = (string?)settings["serverip"] ?? "";
                config.ServerPort = (int?)settings["serverport"] ?? (editionStr == "Bedrock" ? 19132 : 25565);
                config.StartCommand = (string?)settings["startservercommand"] ?? "";
                config.AutoStart = (bool?)settings["autostart"] ?? false;
                config.UseAutoIp = (bool?)settings["useautoip"] ?? true;
                config.UseAutoPort = (bool?)settings["useautopport"] ?? true;
                config.Theme = (string?)settings["theme"] ?? "Sistema";
                config.Backdrop = (string?)settings["backdrop"] ?? "Mica Alt";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error convirtiendo settings a ServerConfig: {ex.Message}");
            }

            return config;
        }

        /// <summary>
        /// Convierte un ServerConfig a JObject
        /// </summary>
        private static JObject ConvertFromServerConfig(ServerConfig config)
        {
            return new JObject
            {
                ["edition"] = config.ServerType.ToString(),
                ["serverlocation"] = config.ServerPath,
                ["serverexecutable"] = config.ExecutablePath,
                ["serverip"] = config.ServerIp,
                ["serverport"] = config.ServerPort,
                ["startservercommand"] = config.StartCommand,
                ["autostart"] = config.AutoStart,
                ["useautoip"] = config.UseAutoIp,
                ["useautoport"] = config.UseAutoPort,
                ["theme"] = config.Theme,
                ["backdrop"] = config.Backdrop
            };
        }

        /// <summary>
        /// Indica si es la primera ejecución de la aplicación
        /// </summary>
        public static bool IsFirstRun()
        {
            var config = GetServerConfig();
            return !config.IsValid;
        }

        /// <summary>
        /// Indica si es la primera ejecución de la aplicación (versión async)
        /// </summary>
        public static async Task<bool> IsFirstRunAsync()
        {
            try
            {
                var config = await GetServerConfigAsync();
                return !config.IsValid;
            }
            catch
            {
                return true; // Si hay error cargando, considerar primera ejecución
            }
        }

        /// <summary>
        /// Resetea toda la configuración
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                if (_serverConfig != null)
                    _serverConfig.PropertyChanged -= OnConfigPropertyChanged;
                _serverConfig = null;
                _hasUnsavedChanges = false;
                
                _fileWatcher?.Dispose();
                _fileWatcher = null;
                
                _saveTimer?.Dispose();
                _saveTimer = null;
                
                _isInitialized = false;
                
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }
            }
        }

        /// <summary>
        /// Libera recursos cuando la aplicación se cierra
        /// </summary>
        public static void Dispose()
        {
            lock (_lock)
            {
                // Guardar cambios pendientes
                if (_hasUnsavedChanges && _serverConfig != null)
                {
                    try
                    {
                        WriteConfigToFile(_serverConfig);
                        System.Diagnostics.Debug.WriteLine("Cambios pendientes guardados al cerrar");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error guardando al cerrar: {ex.Message}");
                    }
                }

                // Limpiar recursos
                if (_serverConfig != null)
                    _serverConfig.PropertyChanged -= OnConfigPropertyChanged;
                _fileWatcher?.Dispose();
                _saveTimer?.Dispose();
            }
        }

        #region Métodos Auxiliares Privados

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);
        }

        private static void LoadConfigFromFile()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    _serverConfig = new ServerConfig();
                    WriteConfigToFile(_serverConfig);
                    return;
                }

                var json = File.ReadAllText(SettingsPath);
                var settings = JObject.Parse(json);
                _serverConfig = ConvertToServerConfig(settings);
                
                System.Diagnostics.Debug.WriteLine("Configuración cargada desde archivo");
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON corrupto, intentando restaurar backup: {ex.Message}");
                RestoreFromBackup();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando configuración: {ex.Message}");
                _serverConfig = new ServerConfig();
            }
        }

        private static void WriteConfigToFile(ServerConfig config)
        {
            var settings = ConvertFromServerConfig(config);
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }

        private static void CreateBackup()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    File.Copy(SettingsPath, BackupPath, true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando backup: {ex.Message}");
            }
        }

        private static void RestoreFromBackup()
        {
            try
            {
                if (File.Exists(BackupPath))
                {
                    File.Copy(BackupPath, SettingsPath, true);
                    LoadConfigFromFile();
                    System.Diagnostics.Debug.WriteLine("Configuración restaurada desde backup");
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restaurando backup: {ex.Message}");
            }

            // Si no hay backup o falla, crear configuración nueva
            _serverConfig = new ServerConfig();
            WriteConfigToFile(_serverConfig);
            System.Diagnostics.Debug.WriteLine("Configuración restablecida a valores por defecto");
        }

        private static void SetupFileWatcher()
        {
            try
            {
                _fileWatcher = new FileSystemWatcher(SettingsDirectory, "Settings.json")
                {
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _fileWatcher.Changed += OnSettingsFileChanged;
                System.Diagnostics.Debug.WriteLine("FileSystemWatcher configurado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando FileSystemWatcher: {ex.Message}");
            }
        }

        private static void SetupAutoSave()
        {
            _saveTimer = new Timer(OnAutoSaveTimer, null, Timeout.Infinite, Timeout.Infinite);
            System.Diagnostics.Debug.WriteLine("Timer de auto-guardado configurado");
        }

        private static void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            lock (_lock)
            {
                _hasUnsavedChanges = true;
                
                // Reiniciar timer de auto-guardado (debounce de 300ms)
                _saveTimer?.Change(300, Timeout.Infinite);
                
                System.Diagnostics.Debug.WriteLine($"Propiedad '{e.PropertyName}' cambiada, programado auto-guardado");
            }
        }

        private static void OnAutoSaveTimer(object? state)
        {
            lock (_lock)
            {
                if (_hasUnsavedChanges && _serverConfig != null)
                {
                    try
                    {
                        WriteConfigToFile(_serverConfig);
                        _hasUnsavedChanges = false;
                        System.Diagnostics.Debug.WriteLine("Auto-guardado completado");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en auto-guardado: {ex.Message}");
                    }
                }
            }
        }

        private static void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
        {
            // Evitar recargas innecesarias durante nuestras propias escrituras
            lock (_lock)
            {
                if (_hasUnsavedChanges) return;

                Task.Delay(100).ContinueWith(_ =>
                {
                    lock (_lock)
                    {
                        try
                        {
                            var previousConfig = _serverConfig;
                            LoadConfigFromFile();
                            
                            // Re-suscribir eventos
                            if (previousConfig != null)
                            {
                                previousConfig.PropertyChanged -= OnConfigPropertyChanged;
                            }
                            if (_serverConfig != null)
                            {
                                _serverConfig.PropertyChanged += OnConfigPropertyChanged;
                            }
                            
                            System.Diagnostics.Debug.WriteLine("Configuración recargada por cambio externo");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error recargando configuración: {ex.Message}");
                        }
                    }
                });
            }
        }

        #endregion

        #region Métodos de compatibilidad - No obsoletos
        
        /// <summary>
        /// Obtiene la configuración como JObject (para compatibilidad)
        /// </summary>
        public static JObject Get()
        {
            return ConvertFromServerConfig(_serverConfig ?? new ServerConfig());
        }

        /// <summary>
        /// Establece la configuración desde JObject (para compatibilidad)
        /// </summary>
        public static void Set(JObject newSettings)
        {
            if (newSettings != null)
            {
                _serverConfig = ConvertToServerConfig(newSettings);
            }
        }

        /// <summary>
        /// Guarda la configuración actual (para compatibilidad)
        /// </summary>
        public static void Save()
        {
            if (_serverConfig != null)
            {
                WriteConfigToFile(_serverConfig);
            }
        }
        
        #endregion
    }
}
