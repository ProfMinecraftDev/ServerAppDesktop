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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ServerAppDesktop.Bedrock.Pages
{
    /// <summary>
    /// Página de exploración y gestión de archivos del servidor.
    /// </summary>
    public sealed partial class FilesPage : Page
    {
        private ServerConfig? _serverConfig;
        private ObservableCollection<ServerFileItem> _serverFiles;
        private bool _isLoading = false;

        public FilesPage()
        {
            InitializeComponent();
            _serverFiles = new ObservableCollection<ServerFileItem>();
            this.Loaded += FilesPage_Loaded;
        }

        private async void FilesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializePageAsync();
        }

        private async Task InitializePageAsync()
        {
            _isLoading = true;

            try
            {
                // Cargar configuración del servidor
                _serverConfig = await SettingsManager.GetServerConfigAsync();

                if (_serverConfig == null || string.IsNullOrWhiteSpace(_serverConfig.ServerPath))
                {
                    await ShowNoConfigurationMessageAsync();
                    return;
                }

                if (!Directory.Exists(_serverConfig.ServerPath))
                {
                    await ShowDirectoryNotFoundMessageAsync();
                    return;
                }

                // Configurar controles
                SetupUI();

                // Cargar archivos del servidor
                await LoadServerFilesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando FilesPage: {ex}");
                await NotifyErrorAsync("Error al cargar los archivos del servidor");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void SetupUI()
        {
            try
            {
                // Configurar el ListView si existe
                if (FilesListView != null)
                {
                    FilesListView.ItemsSource = _serverFiles;
                    _serverFiles.CollectionChanged += (s, e) => UpdateEmptyState();
                }

                // Conectar eventos
                ConnectEventHandlers();

                // Estado inicial
                UpdateEmptyState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando UI: {ex}");
            }
        }

        private void ConnectEventHandlers()
        {
            try
            {
                if (RefreshButton != null)
                    RefreshButton.Click += RefreshButton_Click;

                if (OpenFolderButton != null)
                    OpenFolderButton.Click += OpenFolderButton_Click;

                if (CreateBackupButton != null)
                    CreateBackupButton.Click += CreateBackupButton_Click;

                if (FilesListView != null)
                {
                    FilesListView.DoubleTapped += FilesListView_DoubleTapped;
                    FilesListView.RightTapped += FilesListView_RightTapped;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error conectando eventos: {ex}");
            }
        }

        private async Task LoadServerFilesAsync()
        {
            try
            {
                _serverFiles.Clear();

                if (string.IsNullOrWhiteSpace(_serverConfig?.ServerPath) || !Directory.Exists(_serverConfig.ServerPath))
                    return;

                // Obtener archivos importantes primero
                var importantFiles = new List<string>
                {
                    "server.properties",
                    "permissions.json",
                    "allowlist.json",
                    "whitelist.json",
                    "server.log",
                    "latest.log"
                };

                // Añadir archivos importantes que existen
                foreach (var fileName in importantFiles)
                {
                    try
                    {
                        var filePath = Path.Combine(_serverConfig.ServerPath, fileName);
                        if (File.Exists(filePath))
                        {
                            var fileItem = new ServerFileItem(filePath);
                            _serverFiles.Add(fileItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error procesando archivo {fileName}: {ex}");
                    }
                }

                // Añadir carpetas importantes
                var importantDirs = new List<string> { "worlds", "logs", "backups", "plugins", "behavior_packs", "resource_packs" };
                foreach (var dirName in importantDirs)
                {
                    try
                    {
                        var dirPath = Path.Combine(_serverConfig.ServerPath, dirName);
                        if (Directory.Exists(dirPath))
                        {
                            var dirItem = new ServerFileItem(dirPath) { IsImportant = true };
                            _serverFiles.Add(dirItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error procesando directorio {dirName}: {ex}");
                    }
                }

                // Añadir otros archivos del directorio raíz
                var allFiles = Directory.GetFiles(_serverConfig.ServerPath)
                    .Where(f => !importantFiles.Any(imp => string.Equals(Path.GetFileName(f), imp, StringComparison.OrdinalIgnoreCase)))
                    .Take(50) // Limitar para rendimiento
                    .ToList();

                foreach (var filePath in allFiles)
                {
                    try
                    {
                        var fileItem = new ServerFileItem(filePath);
                        _serverFiles.Add(fileItem);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error procesando archivo {filePath}: {ex}");
                    }
                }

                // Ordenar: importantes primero, luego por nombre
                var sortedFiles = _serverFiles
                    .OrderByDescending(f => f.IsImportant)
                    .ThenBy(f => f.Type == FileItemType.Directory ? 0 : 1)
                    .ThenBy(f => f.Name)
                    .ToList();

                _serverFiles.Clear();
                foreach (var file in sortedFiles)
                {
                    _serverFiles.Add(file);
                }

                UpdateEmptyState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando archivos: {ex}");
                await NotifyErrorAsync($"Error cargando archivos: {ex.Message}");
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            try
            {
                await LoadServerFilesAsync();
                await NotifyInfoAsync("Lista de archivos actualizada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en RefreshButton_Click: {ex}");
                await NotifyErrorAsync("Error al refrescar la lista de archivos.");
            }
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_serverConfig?.ServerPath))
                    return;

                var success = await Windows.System.Launcher.LaunchFolderPathAsync(_serverConfig.ServerPath);
                if (!success)
                {
                    await NotifyWarningAsync("No se pudo abrir la carpeta del servidor");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error abriendo carpeta: {ex}");
                await NotifyErrorAsync("Error al abrir la carpeta del servidor");
            }
        }

        private async void CreateBackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading || string.IsNullOrWhiteSpace(_serverConfig?.ServerPath))
                return;

            try
            {
                _isLoading = true;

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupName = $"backup_{timestamp}";

                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add("*");

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

                var backupFolder = await folderPicker.PickSingleFolderAsync();
                if (backupFolder != null)
                {
                    var backupPath = Path.Combine(backupFolder.Path, backupName);

                    await Task.Run(() =>
                    {
                        if (!string.IsNullOrWhiteSpace(_serverConfig?.ServerPath))
                        {
                            CopyDirectory(_serverConfig.ServerPath, backupPath);
                        }
                    });

                    await NotifySuccessAsync($"Respaldo creado exitosamente en: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando respaldo: {ex}");
                await NotifyErrorAsync($"Error creando respaldo: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void FilesListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (FilesListView?.SelectedItem is ServerFileItem selectedFile)
            {
                _ = HandleFileActionAsync(selectedFile);
            }
        }

        private void FilesListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Aquí se podría implementar un menú contextual
            if (FilesListView?.SelectedItem is ServerFileItem selectedFile)
            {
                System.Diagnostics.Debug.WriteLine($"Menú contextual para: {selectedFile.Name}");
            }
        }

        private async Task HandleFileActionAsync(ServerFileItem fileItem)
        {
            if (fileItem == null) return;

            try
            {
                if (fileItem.Type == FileItemType.Directory)
                {
                    // Abrir carpeta
                    await Windows.System.Launcher.LaunchFolderPathAsync(fileItem.FullPath);
                }
                else if (fileItem.IsEditable)
                {
                    // Mostrar diálogo de edición
                    await ShowFileEditDialogAsync(fileItem);
                }
                else
                {
                    // Abrir con aplicación predeterminada
                    var file = await StorageFile.GetFileFromPathAsync(fileItem.FullPath);
                    await Windows.System.Launcher.LaunchFileAsync(file);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error manejando acción de archivo: {ex}");
                await NotifyErrorAsync($"No se pudo abrir el archivo: {fileItem.Name}");
            }
        }

        private async Task ShowFileEditDialogAsync(ServerFileItem fileItem)
        {
            try
            {
                var fileContent = await File.ReadAllTextAsync(fileItem.FullPath);

                var dialog = new ContentDialog
                {
                    Title = $"Editar - {fileItem.Name}",
                    PrimaryButtonText = "Guardar",
                    SecondaryButtonText = "Cancelar",
                    XamlRoot = this.XamlRoot
                };

                var scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Height = 400
                };

                var textBox = new TextBox
                {
                    Text = fileContent,
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Consolas")
                };

                scrollViewer.Content = textBox;

                dialog.Content = scrollViewer;

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await File.WriteAllTextAsync(fileItem.FullPath, textBox.Text);
                    await NotifySuccessAsync($"Archivo {fileItem.Name} guardado correctamente");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editando archivo: {ex}");
                await NotifyErrorAsync($"Error editando archivo: {ex.Message}");
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            try
            {
                Directory.CreateDirectory(destinationDir);

                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    try
                    {
                        var fileName = Path.GetFileName(file);
                        if (!string.IsNullOrWhiteSpace(fileName))
                        {
                            var destFile = Path.Combine(destinationDir, fileName);
                            File.Copy(file, destFile, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error copiando archivo {file}: {ex}");
                    }
                }

                foreach (var dir in Directory.GetDirectories(sourceDir))
                {
                    try
                    {
                        var dirName = Path.GetFileName(dir);
                        if (!string.IsNullOrWhiteSpace(dirName))
                        {
                            var destDir = Path.Combine(destinationDir, dirName);
                            CopyDirectory(dir, destDir);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error copiando directorio {dir}: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CopyDirectory: {ex}");
                throw;
            }
        }

        private async Task ShowNoConfigurationMessageAsync()
        {
            System.Diagnostics.Debug.WriteLine("No hay configuración del servidor");
            await NotifyWarningAsync("No se encontró configuración del servidor. Configure el servidor primero.");
        }

        private async Task ShowDirectoryNotFoundMessageAsync()
        {
            System.Diagnostics.Debug.WriteLine("Directorio del servidor no encontrado");
            await NotifyErrorAsync("El directorio del servidor no existe. Verifique la configuración.");
        }

        private void UpdateEmptyState()
        {
            try
            {
                bool isEmpty = _serverFiles == null || _serverFiles.Count == 0;

                if (FilesListView != null)
                    FilesListView.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;

                if (EmptyStatePanel != null)
                    EmptyStatePanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando estado vacío: {ex}");
            }
        }

        // Métodos de notificación seguros para la UI thread
        private async Task NotifyErrorAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Aquí tu lógica para mostrar el error
                // Ejemplo: MessageBox, Toast, etc.
                // Puedes reemplazar esto por tu propio método
                await Task.CompletedTask;
            });
        }

        private async Task NotifyWarningAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Tu lógica para warning
                await Task.CompletedTask;
            });
        }

        private async Task NotifyInfoAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Tu lógica para info
                await Task.CompletedTask;
            });
        }

        private async Task NotifySuccessAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Tu lógica para success
                await Task.CompletedTask;
            });
        }
    }
}