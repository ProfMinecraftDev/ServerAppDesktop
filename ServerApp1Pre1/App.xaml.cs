using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ServerApp1Pre1.Utils;

// Code-Behind del App.xaml
// Este archivo contiene la lógica de inicialización de la aplicación y el manejo de eventos de lanzamiento.
// Se utiliza para configurar la ventana principal y manejar el ciclo de vida de la aplicación.

namespace ServerApp1Pre1
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            // No es necesario llamar a base.OnStartup(e) si no se usa StartupUri
            // base.OnStartup(e);

            // Cargar configuración
            var settings = await SettingsManager.LoadAsync();

            // Si falta algún dato esencial, mostrar la ventana de configuración inicial
            bool isConfigMissing = string.IsNullOrWhiteSpace(settings.ServerIp) ||
                                   !settings.ServerPort.HasValue ||
                                   string.IsNullOrWhiteSpace(settings.ServerLocation) ||
                                   string.IsNullOrWhiteSpace(settings.ServerExeFile);

            if (isConfigMissing)
            {
                var firstStartWindow = new FirstStartApp();
                // Usamos ShowDialog() para abrir la ventana de forma modal.
                // La ejecución se detendrá aquí hasta que la ventana se cierre.
                bool? result = firstStartWindow.ShowDialog();

                // Si el usuario guardó la configuración (DialogResult fue establecido a true),
                // entonces procedemos a abrir la ventana principal.
                if (result == true)
                {
                    // Continuar a la ventana principal
                }
                else
                {
                    // Si el usuario cerró la ventana de configuración sin guardar,
                    // la aplicación se cerrará.
                    this.Shutdown();
                    return; // Salir del método
                }
            }

            // Este código se ejecuta si la configuración existe O si se acaba de guardar.
            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
