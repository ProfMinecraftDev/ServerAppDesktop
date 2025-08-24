using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ServerAppDesktop.Models;
using ServerAppDesktop.Services;
using ServerAppDesktop.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop.Bedrock.Pages
{
    /// <summary>
    /// Página principal que muestra el estado del servidor y controles de inicio/parada.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private ServerConfig? _serverConfig;
        private ServerProcessManager? _processManager;
        private Microsoft.UI.Xaml.DispatcherTimer? _refreshTimer;

        public HomePage()
        {
            InitializeComponent();
            Loaded += HomePage_Loaded;
            Unloaded += HomePage_Unloaded;
        }

        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializePageAsync();
        }

        private void HomePage_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupResources();
        }

        private async Task InitializePageAsync()
        {
            try
            {
                // Inicializar SettingsManager
                await SettingsManager.InitializeAsync();
                
                // Cargar configuración del servidor con validación
                _serverConfig = await SettingsManager.GetServerConfigAsync();
                
                // Validar configuración antes de proceder
                var validation = ConfigValidator.ValidateEssentials(_serverConfig);
                if (!validation.IsValid)
                {
                    ShowConfigurationError();
                    return;
                }

                // Inicializar el administrador de procesos
                _processManager = new ServerProcessManager(_serverConfig);
                _processManager.StatusChanged += OnServerStatusChanged;
                _processManager.LogReceived += OnServerLogReceived;

                // Configurar controles de interfaz
                SetupUI();

                // Actualizar información inicial
                await UpdateServerInfoAsync();

                // Configurar timer de actualización
                SetupRefreshTimer();

                // Conectar eventos de botones
                ConnectEventHandlers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando HomePage: {ex.Message}");
                ShowError("Error al cargar la página");
            }
        }

        private void SetupUI()
        {
            // Configurar estado inicial de los controles
            UpdateControlsState(ServerStatus.Stopped);
        }

        private async Task UpdateServerInfoAsync()
        {
            if (_serverConfig == null) return;

            try
            {
                // Actualizar información básica del servidor
                ServerTypeText.Text = _serverConfig.ServerType.ToString();
                ServerPathText.Text = _serverConfig.ServerPath ?? "No configurado";
                ExecutablePathText.Text = Path.GetFileName(_serverConfig.ExecutablePath) ?? "No configurado";
                ServerIpText.Text = _serverConfig.ServerIp ?? "No configurado";
                ServerPortText.Text = _serverConfig.ServerPort.ToString();

                // Actualizar IP si está en modo automático
                if (_serverConfig.UseAutoIp)
                {
                    var currentIp = await NetworkService.GetLocalIpAddressAsync();
                    if (currentIp != _serverConfig.ServerIp)
                    {
                        _serverConfig.ServerIp = currentIp;
                        ServerIpText.Text = currentIp;
                        await SettingsManager.SaveServerConfigAsync(_serverConfig);
                    }
                }

                // Verificar estado actual del proceso
                if (_processManager != null)
                {
                    UpdateStatusDisplay(_processManager.CurrentStatus, _processManager.CurrentStatus.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando información del servidor: {ex.Message}");
            }
        }

        private void SetupRefreshTimer()
        {
            _refreshTimer = new Microsoft.UI.Xaml.DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += async (s, e) => await UpdateServerInfoAsync();
            _refreshTimer.Start();
        }

        private void ConnectEventHandlers()
        {
            if (StartServerButton != null)
                StartServerButton.Click += StartServerButton_Click;

            if (StopServerButton != null)
                StopServerButton.Click += StopServerButton_Click;

            if (RestartServerButton != null)
                RestartServerButton.Click += RestartServerButton_Click;
        }

        private async void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_processManager == null || _processManager.IsRunning || _processManager.IsProcessing)
                return;

            try
            {
                UpdateControlsState(ServerStatus.Starting);
                bool success = await _processManager.StartServerAsync();

                if (!success)
                {
                    ShowError("No se pudo iniciar el servidor. Revise la configuración.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error iniciando servidor: {ex.Message}");
                ShowError("Error al iniciar el servidor");
                UpdateControlsState(ServerStatus.Error);
            }
        }

        private async void StopServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_processManager == null || !_processManager.IsRunning)
                return;

            try
            {
                UpdateControlsState(ServerStatus.Stopping);
                await _processManager.StopServerAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deteniendo servidor: {ex.Message}");
                ShowError("Error al detener el servidor");
                UpdateControlsState(ServerStatus.Error);
            }
        }

        private async void RestartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_processManager == null)
                return;

            try
            {
                if (_processManager.IsRunning)
                {
                    UpdateControlsState(ServerStatus.Stopping);
                    await _processManager.StopServerAsync();

                    // Esperar un momento antes de reiniciar
                    await Task.Delay(2000);
                }

                UpdateControlsState(ServerStatus.Starting);
                bool success = await _processManager.StartServerAsync();

                if (!success)
                {
                    ShowError("No se pudo reiniciar el servidor.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reiniciando servidor: {ex.Message}");
                ShowError("Error al reiniciar el servidor");
                UpdateControlsState(ServerStatus.Error);
            }
        }

        private void OnServerStatusChanged(object? sender, ServerStatusChangedEventArgs e)
        {
            // Ejecutar en el hilo de UI
            DispatcherQueue.TryEnqueue(async () =>
            {
                UpdateStatusDisplay(e.Status, e.Message ?? e.Status.ToString());
                UpdateControlsState(e.Status);

                // Mostrar notificación para cambios importantes
                if (e.Status == ServerStatus.Running)
                {
                    await this.NotifySuccessAsync("Servidor iniciado correctamente");
                }
                else if (e.Status == ServerStatus.Stopped)
                {
                    await this.NotifyInfoAsync("Servidor detenido");
                }
                else if (e.Status == ServerStatus.Error)
                {
                    await this.NotifyErrorAsync(e.Message ?? "Error en el servidor");
                }
            });
        }

        private void OnServerLogReceived(object? sender, ServerLogEventArgs e)
        {
            // Mostrar logs en tiempo real si hay un control de log
            DispatcherQueue.TryEnqueue(() =>
            {
                // Por ahora solo registrar en debug
                System.Diagnostics.Debug.WriteLine($"[{e.Timestamp:HH:mm:ss}] {(e.IsError ? "ERROR" : "INFO")}: {e.LogLine}");
            });
        }

        private void UpdateStatusDisplay(ServerStatus status, string message)
        {
            if (ServerStatusText == null) return;

            ServerStatusText.Text = message ?? GetStatusText(status);

            // Cambiar color según el estado
            var brush = status switch
            {
                ServerStatus.Running => new SolidColorBrush(Microsoft.UI.Colors.Green),
                ServerStatus.Starting => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                ServerStatus.Stopping => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                ServerStatus.Error => new SolidColorBrush(Microsoft.UI.Colors.Red),
                _ => new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };

            ServerStatusText.Foreground = brush;
        }

        private void UpdateControlsState(ServerStatus status)
        {
            bool isRunning = status == ServerStatus.Running;
            bool isStopped = status == ServerStatus.Stopped;
            bool isProcessing = status == ServerStatus.Starting || status == ServerStatus.Stopping;

            if (StartServerButton != null)
                StartServerButton.IsEnabled = isStopped && !isProcessing;

            if (StopServerButton != null)
                StopServerButton.IsEnabled = isRunning && !isProcessing;

            if (RestartServerButton != null)
                RestartServerButton.IsEnabled = (isRunning || isStopped) && !isProcessing;
        }

        private string GetStatusText(ServerStatus status)
        {
            return status switch
            {
                ServerStatus.Stopped => "Detenido",
                ServerStatus.Starting => "Iniciando...",
                ServerStatus.Running => "Ejecutándose",
                ServerStatus.Stopping => "Deteniendo...",
                ServerStatus.Error => "Error",
                _ => "Desconocido"
            };
        }

        private void ShowConfigurationError()
        {
            var errorText = "No se encontró configuración del servidor.\nVaya a Configuraciones para establecer los parámetros del servidor.";

            if (ServerStatusText != null)
            {
                ServerStatusText.Text = "Configuración requerida";
                ServerStatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }

            ShowError(errorText);
        }

        private async void ShowError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"HomePage Error: {message}");
            await this.NotifyErrorAsync(message);
        }

        private void CleanupResources()
        {
            try
            {
                _refreshTimer?.Stop();
                _refreshTimer = null;

                if (_processManager != null)
                {
                    _processManager.StatusChanged -= OnServerStatusChanged;
                    _processManager.LogReceived -= OnServerLogReceived;

                    // No hacer dispose aquí ya que puede estar siendo usado en otras páginas
                    _processManager = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error limpiando recursos: {ex.Message}");
            }
        }
    }
}
