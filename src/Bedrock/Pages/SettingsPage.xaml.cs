using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
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
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop.Bedrock.Pages
{
    /// <summary>
    /// Página de configuraciones del servidor y de la aplicación.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private ServerConfig? _serverConfig;
        private bool _isLoading = false;
        private bool _hasChanges = false;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializePageAsync();
        }

        private void SelectBackdrop(object sender, SelectionChangedEventArgs e)
        {
            string[] backdropsIndex = { "Mica Alt", "Mica", "Desktop Acrylic", "Ninguno" };
            int selected = Backdrop_Selector.SelectedIndex;

            if (selected >= 0 && selected < backdropsIndex.Length)
            {
                switch (backdropsIndex[selected])
                {
                    case "Mica Alt":
                        App.Window.SystemBackdrop = new MicaBackdrop
                        {
                            Kind = MicaKind.BaseAlt
                        };
                        break;
                    case "Mica":
                        App.Window.SystemBackdrop = new MicaBackdrop
                        {
                            Kind = MicaKind.Base
                        };
                        break;
                    case "Desktop Acrylic":
                        App.Window.SystemBackdrop = new DesktopAcrylicBackdrop();
                        break;
                }
            }
        }

        private void SelectTheme(object sender, SelectionChangedEventArgs e)
        {
            int index = Theme_Selector.SelectedIndex;
            var app = (App)Application.Current;

            switch (index)
            {
                case 0: // System/Default
                default:
                    app.ApplyGlobalTheme(ElementTheme.Default);
                    break;

                case 2: // Dark
                    app.ApplyGlobalTheme(ElementTheme.Dark);
                    break;

                case 1: // Light
                    app.ApplyGlobalTheme(ElementTheme.Light);
                    break;
            }
        }

        private async Task InitializePageAsync()
        {
            _isLoading = true;
            
            try
            {
                // Inicializar SettingsManager si no está inicializado
                await SettingsManager.InitializeAsync();
                
                // Cargar configuración del servidor con auto-detección de red
                var baseConfig = await SettingsManager.GetServerConfigAsync();
                _serverConfig = await SettingsManager.ApplyAutoNetworkConfigAsync(baseConfig);
                
                // Suscribir para auto-guardado
                SettingsManager.Subscribe(_serverConfig);

                // Configurar controles de interfaz
                await LoadServerSettings();
                ConnectServerEventHandlers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando SettingsPage: {ex.Message}");
                ShowError("Error al cargar las configuraciones");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadServerSettings()
        {
            if (_serverConfig == null) return;

            try
            {
                // Actualizar campos de configuración del servidor
                if (ServerPathText != null)
                    ServerPathText.Text = _serverConfig.ServerPath ?? "";
                
                if (ExecutablePathText != null)
                    ExecutablePathText.Text = _serverConfig.ExecutablePath ?? "";
                
                if (ServerIpText != null)
                    ServerIpText.Text = _serverConfig.ServerIp ?? "";
                
                if (ServerPortText != null)
                    ServerPortText.Text = _serverConfig.ServerPort.ToString();

                // Configurar tipo de servidor
                if (ServerTypeSelector != null)
                {
                    ServerTypeSelector.SelectedIndex = _serverConfig.ServerType == ServerType.Java ? 1 : 0;
                }

                // Configurar checkboxes
                if (AutoIpCheckBox != null)
                    AutoIpCheckBox.IsChecked = _serverConfig.UseAutoIp;
                
                if (AutoPortCheckBox != null)
                    AutoPortCheckBox.IsChecked = _serverConfig.UseAutoPort;

                // Actualizar IP automática si está habilitada
                if (_serverConfig.UseAutoIp)
                {
                    var currentIp = await NetworkService.GetLocalIpAddressAsync();
                    _serverConfig.ServerIp = currentIp;
                    if (ServerIpText != null)
                        ServerIpText.Text = currentIp;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando configuración del servidor: {ex.Message}");
            }
        }

        private void ConnectServerEventHandlers()
        {
            // Conectar eventos de botones
            if (ServerFolderPicker != null)
                ServerFolderPicker.Click += ServerFolderPicker_Click;
            
            if (ExecutablePicker != null)
                ExecutablePicker.Click += ExecutablePicker_Click;
            
            if (SaveSettingsButton != null)
                SaveSettingsButton.Click += SaveSettingsButton_Click;

            // Eventos de cambio de configuración
            if (ServerTypeSelector != null)
                ServerTypeSelector.SelectionChanged += ServerTypeSelector_SelectionChanged;
            
            if (AutoIpCheckBox != null)
            {
                AutoIpCheckBox.Checked += AutoIpCheckBox_Checked;
                AutoIpCheckBox.Unchecked += AutoIpCheckBox_Unchecked;
            }
            
            if (AutoPortCheckBox != null)
            {
                AutoPortCheckBox.Checked += AutoPortCheckBox_Checked;
                AutoPortCheckBox.Unchecked += AutoPortCheckBox_Unchecked;
            }

            // Eventos de cambio de texto
            if (ServerIpText != null)
                ServerIpText.TextChanged += OnServerConfigChanged;
            
            if (ServerPortText != null)
                ServerPortText.TextChanged += OnServerConfigChanged;
        }

        private async void ServerFolderPicker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serverConfig == null) return;

                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add("*");

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    _serverConfig.ServerPath = folder.Path;
                    if (ServerPathText != null)
                        ServerPathText.Text = folder.Path;
                    
                    await TryAutoDetectSettings();
                    MarkAsChanged();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seleccionando carpeta: {ex.Message}");
                ShowError("Error al seleccionar la carpeta del servidor");
            }
        }

        private async void ExecutablePicker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serverConfig == null) return;

                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                
                if (_serverConfig.ServerType == ServerType.Java)
                {
                    filePicker.FileTypeFilter.Add(".jar");
                }
                else
                {
                    filePicker.FileTypeFilter.Add(".exe");
                }

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

                var file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    _serverConfig.ExecutablePath = file.Path;
                    if (ExecutablePathText != null)
                        ExecutablePathText.Text = file.Path;
                    
                    MarkAsChanged();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seleccionando ejecutable: {ex.Message}");
                ShowError("Error al seleccionar el ejecutable del servidor");
            }
        }

        private async void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serverConfig == null || !ValidateConfiguration())
                {
                    ShowError("Por favor, complete todos los campos requeridos");
                    return;
                }

                // Actualizar configuración con valores de la UI
                UpdateConfigFromUI();

                // Guardar la configuración
                await SettingsManager.SaveServerConfigAsync(_serverConfig);
                
                _hasChanges = false;
                UpdateSaveButtonState();
                
                ShowSuccess("Configuración guardada exitosamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando configuración: {ex.Message}");
                ShowError("Error al guardar la configuración");
            }
        }

        private async void ServerTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading || _serverConfig == null) return;

            var selectedIndex = ServerTypeSelector.SelectedIndex;
            var newType = selectedIndex == 1 ? ServerType.Java : ServerType.Bedrock;
            
            if (_serverConfig.ServerType != newType)
            {
                _serverConfig.ServerType = newType;
                
                // Limpiar ejecutable al cambiar tipo
                _serverConfig.ExecutablePath = "";
                
                await TryAutoDetectSettings();
                MarkAsChanged();
            }
        }

        private async void AutoIpCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoading || _serverConfig == null) return;

            _serverConfig.UseAutoIp = true;
            var currentIp = await NetworkService.GetLocalIpAddressAsync();
            _serverConfig.ServerIp = currentIp;
            
            if (ServerIpText != null)
                ServerIpText.Text = currentIp;
            
            MarkAsChanged();
        }

        private void AutoIpCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isLoading || _serverConfig == null) return;

            _serverConfig.UseAutoIp = false;
            MarkAsChanged();
        }

        private async void AutoPortCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoading || _serverConfig == null) return;

            _serverConfig.UseAutoPort = true;
            await TryAutoDetectPort();
            MarkAsChanged();
        }

        private void AutoPortCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isLoading || _serverConfig == null) return;

            _serverConfig.UseAutoPort = false;
            MarkAsChanged();
        }

        private void OnServerConfigChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoading) return;
            MarkAsChanged();
        }

        private async Task TryAutoDetectSettings()
        {
            if (_serverConfig == null || string.IsNullOrEmpty(_serverConfig.ServerPath))
                return;

            try
            {
                // Auto-detectar ejecutable
                await TryAutoDetectExecutable();
                
                // Auto-detectar puerto si está habilitado
                if (_serverConfig.UseAutoPort)
                {
                    await TryAutoDetectPort();
                }
                
                // Actualizar UI
                await LoadServerSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en detección automática: {ex.Message}");
            }
        }

        private async Task TryAutoDetectExecutable()
        {
            if (_serverConfig == null || string.IsNullOrEmpty(_serverConfig.ServerPath))
                return;

            try
            {
                await Task.Run(() =>
                {
                    string[] patterns = _serverConfig.ServerType == ServerType.Java
                        ? new[] { "server.jar", "minecraft_server*.jar", "*.jar" }
                        : new[] { "bedrock_server.exe", "*.exe" };

                    foreach (var pattern in patterns)
                    {
                        var files = Directory.GetFiles(_serverConfig.ServerPath, pattern);
                        if (files.Length > 0)
                        {
                            _serverConfig.ExecutablePath = files[0];
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detectando ejecutable: {ex.Message}");
            }
        }

        private async Task TryAutoDetectPort()
        {
            if (_serverConfig == null || string.IsNullOrEmpty(_serverConfig.ServerPath))
                return;

            try
            {
                string propertiesFile = Path.Combine(_serverConfig.ServerPath, "server.properties");

                if (File.Exists(propertiesFile))
                {
                    var port = await PropertiesFileService.GetServerPortAsync(propertiesFile);
                    if (port.HasValue)
                    {
                        _serverConfig.ServerPort = port.Value;
                    }
                }
                else
                {
                    // Puerto por defecto según tipo de servidor
                    _serverConfig.ServerPort = _serverConfig.ServerType == ServerType.Java ? 25565 : 19132;
                }

                if (ServerPortText != null)
                    ServerPortText.Text = _serverConfig.ServerPort.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detectando puerto: {ex.Message}");
                // Puerto por defecto en caso de error
                _serverConfig.ServerPort = _serverConfig.ServerType == ServerType.Java ? 25565 : 19132;
            }
        }

        private void UpdateConfigFromUI()
        {
            if (_serverConfig == null) return;

            if (ServerIpText != null)
                _serverConfig.ServerIp = ServerIpText.Text.Trim();
            
            if (ServerPortText != null && int.TryParse(ServerPortText.Text.Trim(), out int port))
                _serverConfig.ServerPort = port;
        }

        private bool ValidateConfiguration()
        {
            if (_serverConfig == null)
                return false;

            if (string.IsNullOrEmpty(_serverConfig.ServerPath) || !Directory.Exists(_serverConfig.ServerPath))
                return false;
            
            if (string.IsNullOrEmpty(_serverConfig.ExecutablePath) || !File.Exists(_serverConfig.ExecutablePath))
                return false;
            
            if (string.IsNullOrEmpty(_serverConfig.ServerIp))
                return false;
            
            if (_serverConfig.ServerPort <= 0 || _serverConfig.ServerPort > 65535)
                return false;

            return true;
        }

        private void MarkAsChanged()
        {
            _hasChanges = true;
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            if (SaveSettingsButton != null)
            {
                SaveSettingsButton.IsEnabled = _hasChanges && !_isLoading;
            }
        }

        private async void ShowError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"SettingsPage Error: {message}");
            await this.NotifyErrorAsync(message);
        }

        private async void ShowSuccess(string message)
        {
            System.Diagnostics.Debug.WriteLine($"SettingsPage Success: {message}");
            await this.NotifySuccessAsync(message);
        }
    }
}