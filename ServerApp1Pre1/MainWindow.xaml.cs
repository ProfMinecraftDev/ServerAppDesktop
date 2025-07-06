using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ServerApp1Pre1.Nav;

// Code-behind de la ventana principal de la aplicacion
// Este archivo define la logica de la ventana principal, incluyendo la navegacion y el titulo de la aplicacion.
namespace ServerApp1Pre1
{
    // La clase MainWindow hereda de Window y también implementa INotifyPropertyChanged
    // para notificar cambios en las propiedades, como el título de la ventana.
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _title = "Server App (Preview)";
        private string _selectedPage = "Home";

        public new string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (_selectedPage != value)
                {
                    _selectedPage = value;
                    OnPropertyChanged();
                }
            }
        }

        // Constructor de la clase MainWindow
        // Aquí se inicializa la ventana, se configura la barra de título y se establece la navegación inicial.
        public MainWindow()
        {
            // Inicializa los componentes de la ventana
            InitializeComponent();

            // Configurar el DataContext para el binding
            DataContext = this;

            // Navegación inicial - navegar a la página de inicio
            NavigateToPage(typeof(HomePage));
            SelectedPage = "Home";
        }

        // Eventos de navegación
        private void HomeNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(HomePage));
            SelectedPage = "Home";
        }

        private void FilesNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(FilesPage));
            SelectedPage = "Files";
        }

        private void SettingsNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(SettingsPage));
            SelectedPage = "Settings";
        }

        // Método auxiliar para navegar a una página
        private void NavigateToPage(Type pageType)
        {
            try
            {
                contentFrame.Navigate(Activator.CreateInstance(pageType));
            }
            catch (Exception)
            {
            }
        }

        // Implementación de INotifyPropertyChanged para notificar cambios en las propiedades
        public event PropertyChangedEventHandler? PropertyChanged;

        // Método que se llama para notificar a la interfaz de usuario que una propiedad ha cambiado
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
