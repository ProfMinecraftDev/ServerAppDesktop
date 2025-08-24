using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ServerAppDesktop.Bedrock;
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

namespace ServerAppDesktop.InitialSettings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InitialSettingsMainPage : Page
    {
        private readonly ServerConfig _config = new();
        private bool _isConfiguring = false;

        public InitialSettingsMainPage()
        {
            InitializeComponent();
            App.Window.Title = "Server App Desktop (Preview) - Configuración Inicial";
            Loaded += InitialSettingsMainPage_Loaded;
        }

        private async void InitialSettingsMainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Inicializar SettingsManager
                await SettingsManager.InitializeAsync();
                
                // Aplicar configuración automática de red
                var updatedConfig = await SettingsManager.ApplyAutoNetworkConfigAsync(_config);
                
                // Copiar propiedades de la configuración actualizada
                _config.ServerIp = updatedConfig.ServerIp;
                _config.ServerPort = updatedConfig.ServerPort;
                _config.UseAutoIp = updatedConfig.UseAutoIp;
                _config.UseAutoPort = updatedConfig.UseAutoPort;
                
                // Suscribir para auto-guardado
                SettingsManager.Subscribe(_config);

                // Conectar controles con eventos
                ConnectEventHandlers();
                UpdateUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando: {ex.Message}");
            }
        }

        private void ConnectEventHandlers()
        {
            ServerFolderPicker.Click += ServerFolderPicker_Click;
            ExecutablePicker.Click += ExecutablePicker_Click;
            ServerTypeRadio.SelectionChanged += ServerTypeRadio_SelectionChanged;
            AutoIpCheckBox.Checked += AutoIpCheckBox_Checked;
            AutoIpCheckBox.Unchecked += AutoIpCheckBox_Unchecked;
            AutoPortCheckBox.Checked += AutoPortCheckBox_Checked;
            AutoPortCheckBox.Unchecked += AutoPortCheckBox_Unchecked;
        }

        private void UpdateUI()
        {
            try
            {
                // Actualizar controles con la configuración actual
                ServerFolderPathText.Text = _config.ServerPath ?? "No seleccionado";
                ExecutablePathText.Text = _config.ExecutablePath ?? "No seleccionado";
                ServerPortText.Text = _config.ServerPort.ToString();
                ServerIpText.Text = _config.ServerIp ?? "";
                AutoIpCheckBox.IsChecked = _config.UseAutoIp;
                AutoPortCheckBox.IsChecked = _config.UseAutoPort;

                // Configurar tipo de servidor
                if (_config.ServerType == ServerType.Bedrock)
                    ServerTypeRadio.SelectedIndex = 0;
                else if (_config.ServerType == ServerType.Java)
                    ServerTypeRadio.SelectedIndex = 1;

                // Validar si se puede completar la configuración
                ValidateConfiguration();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando UI: {ex.Message}");
            }
        }

        private void ValidateConfiguration()
        {
            bool isValid = !string.IsNullOrEmpty(_config.ServerPath) &&
                          !string.IsNullOrEmpty(_config.ExecutablePath) &&
                          Directory.Exists(_config.ServerPath) &&
                          File.Exists(_config.ExecutablePath) &&
                          _config.ServerPort > 0 &&
                          !string.IsNullOrEmpty(_config.ServerIp);

            FinishButton.IsEnabled = isValid && !_isConfiguring;
        }

        private async void ServerFolderPicker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add("*");

                // Inicializar el selector con la ventana actual
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    _config.ServerPath = folder.Path;
                    await TryAutoDetectSettings();
                    UpdateUI();
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
                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                
                if (_config.ServerType == ServerType.Java)
                {
                    filePicker.FileTypeFilter.Add(".jar");
                }
                else
                {
                    filePicker.FileTypeFilter.Add(".exe");
                }

                // Inicializar el selector con la ventana actual
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

                var file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    _config.ExecutablePath = file.Path;
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seleccionando ejecutable: {ex.Message}");
                ShowError("Error al seleccionar el ejecutable del servidor");
            }
        }

        private async void ServerTypeRadio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerTypeRadio.SelectedIndex == 0)
                _config.ServerType = ServerType.Bedrock;
            else if (ServerTypeRadio.SelectedIndex == 1)
                _config.ServerType = ServerType.Java;

            // Limpiar rutas al cambiar tipo
            _config.ExecutablePath = string.Empty;
            await TryAutoDetectSettings();
            UpdateUI();
        }

        private async void AutoIpCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _config.UseAutoIp = true;
            _config.ServerIp = await NetworkService.GetLocalIpAddressAsync();
            UpdateUI();
        }

        private void AutoIpCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _config.UseAutoIp = false;
            UpdateUI();
        }

        private async void AutoPortCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _config.UseAutoPort = true;
            await TryAutoDetectPort();
            UpdateUI();
        }

        private void AutoPortCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _config.UseAutoPort = false;
            UpdateUI();
        }

        private async Task TryAutoDetectSettings()
        {
            if (string.IsNullOrEmpty(_config.ServerPath))
                return;

            try
            {
                // Auto-detectar ejecutable
                await TryAutoDetectExecutable();
                
                // Auto-detectar puerto si está habilitado
                if (_config.UseAutoPort)
                {
                    await TryAutoDetectPort();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en detección automática: {ex.Message}");
            }
        }

        private Task TryAutoDetectExecutable()
        {
            if (string.IsNullOrEmpty(_config.ServerPath))
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                try
                {
                    string[] patterns = _config.ServerType == ServerType.Java 
                        ? ["server.jar", "minecraft_server*.jar", "*.jar"]
                        : ["bedrock_server.exe", "*.exe"];

                    foreach (var pattern in patterns)
                    {
                        var files = Directory.GetFiles(_config.ServerPath, pattern);
                        if (files.Length > 0)
                        {
                            _config.ExecutablePath = files[0];
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error detectando ejecutable: {ex.Message}");
                }
            });
        }

        private async Task TryAutoDetectPort()
        {
            if (string.IsNullOrEmpty(_config.ServerPath))
                return;

            try
            {
                string propertiesFile = _config.ServerType == ServerType.Java
                    ? Path.Combine(_config.ServerPath, "server.properties")
                    : Path.Combine(_config.ServerPath, "server.properties");

                if (File.Exists(propertiesFile))
                {
                    var port = await PropertiesFileService.GetServerPortAsync(propertiesFile);
                    if (port.HasValue)
                    {
                        _config.ServerPort = port.Value;
                    }
                }
                else
                {
                    // Puerto por defecto según tipo de servidor
                    _config.ServerPort = _config.ServerType == ServerType.Java ? 25565 : 19132;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detectando puerto: {ex.Message}");
                // Puerto por defecto en caso de error
                _config.ServerPort = _config.ServerType == ServerType.Java ? 25565 : 19132;
            }
        }

        private void ShowError(string message)
        {
            // Mostrar error en la UI (por ahora solo debug)
            System.Diagnostics.Debug.WriteLine($"Error: {message}");
        }

        private async void Iniciar_App(object sender, RoutedEventArgs e)
        {
            if (_isConfiguring) return;

            try
            {
                _isConfiguring = true;
                ValidateConfiguration();

                // Guardar la configuración
                await SettingsManager.SaveServerConfigAsync(_config);

                // Mostrar progreso
                LayoutRoot.Children.Remove(MainScrollView);

                var ring = new ProgressRing
                {
                    IsIndeterminate = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 100,
                    Height = 100
                };
                var text = new TextBlock
                {
                    Text = "Guardando configuración...",
                    Style = (Style)Application.Current.Resources["TitleTextBlockStyle"],
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var stackPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { ring, text },
                    Spacing = 10
                };
                Grid.SetRow(stackPanel, 0);

                LayoutRoot.Children.Add(stackPanel);
                LayoutRoot.Children.Remove(FinishButton);
                await Task.Delay(2000);

                LayoutRoot.Children.Remove(stackPanel);
                App.Window.M_Frame.Navigate(typeof(BedrockMainPage));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error completando configuración: {ex.Message}");
                ShowError("Error al guardar la configuración");
                _isConfiguring = false;
                ValidateConfiguration();
            }
        }
    }
}

