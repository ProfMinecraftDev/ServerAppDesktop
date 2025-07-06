using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ServerApp1Pre1.Utils;

namespace ServerApp1Pre1.Nav
{
    // Página principal para el control del servidor
    public partial class HomePage : Page
    {
        private AppSettings? _settings; // Configuración cargada
        private Process? _serverProcess; // Proceso del servidor

        public HomePage()
        {
            InitializeComponent();
            Loaded += HomePage_Loaded; // Evento al cargar la página
        }

        // Evento al cargar la página: carga configuración y actualiza UI
        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            _settings = await SettingsManager.LoadAsync();
            IpServidorText.Text = _settings.ServerIp ?? "No definido";
            PuertoServidorText.Text = _settings.ServerPort?.ToString() ?? "No definido";
            UbicacionServidorText.Text = _settings.ServerLocation ?? "No definido";
            EstadoServidorText.Text = (_serverProcess != null && !_serverProcess.HasExited) ? "Conectado" : "Desconectado";
        }

        // Refactoriza la lógica de iniciar el servidor para reutilizarla
        private async Task IniciarServidorAsync()
        {
            if (_settings == null || string.IsNullOrWhiteSpace(_settings.ServerExeFile) || !File.Exists(_settings.ServerExeFile))
            {
                MessageBox.Show("No se ha configurado correctamente el ejecutable del servidor.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_serverProcess == null || _serverProcess.HasExited)
            {
                await Task.Run(() =>
                {
                    _serverProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = _settings.ServerExeFile,
                            WorkingDirectory = Path.GetDirectoryName(_settings.ServerExeFile) ?? "",
                            UseShellExecute = true
                        }
                    };
                    _serverProcess.Start();
                });
                EstadoServidorText.Text = "Conectado";
            }
        }

        // Refactoriza la lógica de detener el servidor para reutilizarla
        private async Task DetenerServidorAsync()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        _serverProcess.Kill();
                        _serverProcess.WaitForExit();
                    });
                    EstadoServidorText.Text = "Desconectado";
                }
                catch
                {
                    MessageBox.Show("No se pudo detener el servidor.",
                                  "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Modifica los handlers de los botones para usar los métodos refactorizados
        private async void IniciarButton_Click(object sender, RoutedEventArgs e)
        {
            await IniciarServidorAsync();
        }

        private async void DetenerButton_Click(object sender, RoutedEventArgs e)
        {
            await DetenerServidorAsync();
        }

        private async void ReiniciarButton_Click(object sender, RoutedEventArgs e)
        {
            await DetenerServidorAsync();
            await IniciarServidorAsync();
        }
    }
}
