// Code-behind: Utils/FirstStartApp.xaml.cs
// Este archivo contiene la lógica para la ventana de configuración inicial de la aplicación.
// Permite al usuario ingresar la IP del servidor, el puerto, la ubicación del servidor y el archivo ejecutable del servidor.
// Almacena estos datos en la configuración de la aplicación y abre la ventana principal.
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ServerApp1Pre1.Utils
{
    // Clase para la ventana de configuración inicial de la aplicación
    public partial class FirstStartApp : Window
    {
        // Constructor de la clase
        // Inicializa los componentes de la ventana
        public FirstStartApp()
        {
            InitializeComponent(); // Inicializa los componentes de la ventana
        }

        // Evento que se ejecuta al guardar la configuración
        // Carga la configuración inicial del servidor si existe, de lo contrario, deja los campos vacíos
        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validación de datos de entrada
            if (string.IsNullOrWhiteSpace(IpTextBox.Text) ||
                string.IsNullOrWhiteSpace(PortTextBox.Text) ||
                string.IsNullOrWhiteSpace(LocationTextBox.Text) ||
                string.IsNullOrWhiteSpace(ExeFileTextBox.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(PortTextBox.Text, out var port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("El puerto ingresado no es válido. Debe ser un número entre 1 y 65535.", "Puerto no válido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var settings = new AppSettings
            {
                ServerIp = IpTextBox.Text,
                ServerPort = port,
                ServerLocation = LocationTextBox.Text,
                ServerExeFile = ExeFileTextBox.Text
            };

            // 2. Bloque Try-Catch para un guardado seguro
            try
            {
                await SettingsManager.SaveAsync(settings);

                // Si el guardado es exitoso, establece el resultado y cierra la ventana.
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                // Si ocurre un error, informa al usuario y no cierres la ventana.
                MessageBox.Show($"Ocurrió un error al guardar la configuración:\n\n{ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento que se ejecuta al seleccionar la carpeta del servidor
        // Abre un selector de carpetas para que el usuario elija la ubicación del servidor
        private void SeleccionarCarpeta_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Seleccionar carpeta del servidor"
            };

            if (dialog.ShowDialog() == true)
            {
                LocationTextBox.Text = dialog.FolderName; // Si el usuario selecciona una carpeta, establece la ruta de la carpeta en el TextBox correspondiente
            }
        }

        // Evento que se ejecuta al seleccionar el archivo ejecutable del servidor
        private void SeleccionarArchivo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar archivo ejecutable del servidor",
                Filter = "Archivos ejecutables (*.exe)|*.exe|Todos los archivos (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == true)
            {
                ExeFileTextBox.Text = dialog.FileName; // Si el usuario selecciona un archivo, establece la ruta del archivo en el TextBox correspondiente
            }
        }
    }
}