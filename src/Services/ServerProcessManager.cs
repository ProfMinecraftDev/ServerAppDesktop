using ServerAppDesktop.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerAppDesktop.Services
{
    /// <summary>
    /// Estado del servidor: Porque saber si tu servidor está vivo o muerto es algo importante
    /// </summary>
    public enum ServerStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Error
    }

    public class ServerStatusChangedEventArgs : EventArgs
    {
        public ServerStatus Status { get; set; }
        public string? Message { get; set; }
    }

    public class ServerLogEventArgs : EventArgs
    {
        public string LogLine { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsError { get; set; }
    }

    /// <summary>
    /// Administrador de procesos del servidor: La niñera de servidores Minecraft rebeldes desde 2024
    /// Maneja el inicio, detención y las rabietas ocasionales de tu proceso de servidor
    /// </summary>
    public class ServerProcessManager
    {
        private Process? _serverProcess;
        private ServerConfig? _config;
        private ServerStatus _currentStatus = ServerStatus.Stopped;
        private readonly object _processLock = new object();
        private CancellationTokenSource? _cancellationTokenSource;

        public event EventHandler<ServerStatusChangedEventArgs>? StatusChanged;
        public event EventHandler<ServerLogEventArgs>? LogReceived;

        public ServerStatus CurrentStatus
        {
            get { return _currentStatus; }
            private set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    StatusChanged?.Invoke(this, new ServerStatusChangedEventArgs
                    {
                        Status = value,
                        Message = GetStatusMessage(value)
                    });
                }
            }
        }

        public bool IsRunning => _currentStatus == ServerStatus.Running;
        public bool IsProcessing => _currentStatus == ServerStatus.Starting || _currentStatus == ServerStatus.Stopping;

        public ServerProcessManager(ServerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void UpdateConfig(ServerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<bool> StartServerAsync()
        {
            if (IsProcessing || IsRunning)
            {
                return false;
            }

            try
            {
                CurrentStatus = ServerStatus.Starting;

                if (!ValidateConfiguration())
                {
                    CurrentStatus = ServerStatus.Error;
                    return false;
                }

                _cancellationTokenSource = new CancellationTokenSource();

                lock (_processLock)
                {
                    _serverProcess = CreateServerProcess();
                    if (_serverProcess == null)
                    {
                        CurrentStatus = ServerStatus.Error;
                        return false;
                    }
                }

                // Conectar manejadores de eventos para comunicación del proceso
                _serverProcess.OutputDataReceived += OnOutputDataReceived;
                _serverProcess.ErrorDataReceived += OnErrorDataReceived;
                _serverProcess.Exited += OnProcessExited;

                bool started = _serverProcess.Start();
                if (!started)
                {
                    CurrentStatus = ServerStatus.Error;
                    DisposeProcess();
                    return false;
                }

                // Empezar a escuchar la cátedra del servidor
                _serverProcess.BeginOutputReadLine();
                _serverProcess.BeginErrorReadLine();

                // Darle al servidor un momento para acomodarse (como una mascota nueva)
                await Task.Delay(2000, _cancellationTokenSource.Token);

                if (_serverProcess.HasExited)
                {
                    CurrentStatus = ServerStatus.Error;
                    return false;
                }

                CurrentStatus = ServerStatus.Running;
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error iniciando servidor: {ex.Message}");
                CurrentStatus = ServerStatus.Error;
                DisposeProcess();
                return false;
            }
        }

        public Task<bool> StopServerAsync()
        {
            if (!IsRunning && !IsProcessing)
            {
                return Task.FromResult(true);
            }

            return Task.Run(() =>
            {
                try
                {
                    CurrentStatus = ServerStatus.Stopping;

                    _cancellationTokenSource?.Cancel();

                    lock (_processLock)
                    {
                        if (_serverProcess != null && !_serverProcess.HasExited)
                        {
                            // Probar el enfoque educado primero (pidiendo por favor)
                            try
                            {
                                _serverProcess.StandardInput?.WriteLine("stop");
                                _serverProcess.StandardInput?.WriteLine("exit");
                            }
                            catch
                            {
                                // El servidor no quiere escuchar, hora del plan B
                            }

                            // Darle 10 segundos para despedirse
                            bool gracefulExit = _serverProcess.WaitForExit(10000);

                            if (!gracefulExit && !_serverProcess.HasExited)
                            {
                                // Hora de la opción nuclear 💥
                                _serverProcess.Kill();
                                _serverProcess.WaitForExit(5000);
                            }
                        }
                    }

                    DisposeProcess();
                    CurrentStatus = ServerStatus.Stopped;
                    return true;
                }
                catch (Exception ex)
                {
                    LogError($"Error deteniendo servidor: {ex.Message}");
                    CurrentStatus = ServerStatus.Error;
                    DisposeProcess();
                    return false;
                }
            });
        }

        public async Task SendCommandAsync(string command)
        {
            if (!IsRunning || _serverProcess?.StandardInput == null)
            {
                return;
            }

            try
            {
                await _serverProcess.StandardInput.WriteLineAsync(command);
                LogMessage($"Comando enviado: {command}", false);
            }
            catch (Exception ex)
            {
                LogError($"Error enviando comando '{command}': {ex.Message}");
            }
        }

        private bool ValidateConfiguration()
        {
            if (_config == null)
            {
                LogError("Configuración no establecida");
                return false;
            }

            if (string.IsNullOrEmpty(_config.ExecutablePath) || !File.Exists(_config.ExecutablePath))
            {
                LogError("Ejecutable del servidor no encontrado");
                return false;
            }

            if (string.IsNullOrEmpty(_config.ServerPath) || !Directory.Exists(_config.ServerPath))
            {
                LogError("Directorio del servidor no encontrado");
                return false;
            }

            return true;
        }

        private Process? CreateServerProcess()
        {
            try
            {
                if (_config == null)
                    return null;
                    
                var process = new Process();
                var startInfo = new ProcessStartInfo();

                if (_config.ServerType == ServerType.Java)
                {
                    // Servidor Java: Requiere más RAM que Chrome 😅
                    startInfo.FileName = "java";
                    startInfo.Arguments = $"-Xmx2G -Xms1G -jar \"{_config.ExecutablePath}\" nogui";
                }
                else
                {
                    // Servidor Bedrock: Eficiente y nativo como C++
                    startInfo.FileName = _config.ExecutablePath;
                    startInfo.Arguments = "";
                }

                startInfo.WorkingDirectory = _config.ServerPath;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                startInfo.StandardErrorEncoding = Encoding.UTF8;

                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;

                return process;
            }
            catch (Exception ex)
            {
                LogError($"Error creando proceso: {ex.Message}");
                return null;
            }
        }

        private void OnOutputDataReceived(object? sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LogMessage(e.Data, false);
            }
        }

        private void OnErrorDataReceived(object? sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LogMessage(e.Data, true);
            }
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            var exitCode = _serverProcess?.ExitCode ?? -1;
            LogMessage($"Servidor detenido con código de salida: {exitCode}", exitCode != 0);
            
            DisposeProcess();
            CurrentStatus = ServerStatus.Stopped;
        }

        private void LogMessage(string message, bool isError)
        {
            LogReceived?.Invoke(this, new ServerLogEventArgs
            {
                LogLine = message,
                Timestamp = DateTime.Now,
                IsError = isError
            });
        }

        private void LogError(string error)
        {
            LogMessage($"ERROR: {error}", true);
        }

        private string GetStatusMessage(ServerStatus status)
        {
            return status switch
            {
                ServerStatus.Stopped => "Servidor detenido",
                ServerStatus.Starting => "Iniciando servidor...",
                ServerStatus.Running => "Servidor ejecutándose",
                ServerStatus.Stopping => "Deteniendo servidor...",
                ServerStatus.Error => "Error en el servidor",
                _ => "Estado desconocido"
            };
        }

        private void DisposeProcess()
        {
            lock (_processLock)
            {
                if (_serverProcess != null)
                {
                    try
                    {
                        _serverProcess.OutputDataReceived -= OnOutputDataReceived;
                        _serverProcess.ErrorDataReceived -= OnErrorDataReceived;
                        _serverProcess.Exited -= OnProcessExited;

                        if (!_serverProcess.HasExited)
                        {
                            _serverProcess.Kill();
                        }

                        _serverProcess.Dispose();
                    }
                    catch
                    {
                        // ¿Errores de dispose del proceso? De eso no hablamos 🤫
                    }
                    finally
                    {
                        _serverProcess = null;
                    }
                }
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        public void Dispose()
        {
            if (IsRunning || IsProcessing)
            {
                Task.Run(async () => await StopServerAsync()).Wait(15000);
            }

            DisposeProcess();
        }
    }
}
