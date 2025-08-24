using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ServerAppDesktop.Bedrock;
using ServerAppDesktop.InitialSettings;
using ServerAppDesktop.Services;
using ServerAppDesktop.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public SystemBackdropConfiguration BackdropConfig { get; } = new();

        private static MainWindow? _window;
        public static MainWindow Window => _window ?? throw new InvalidOperationException("Window no inicializada");

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            
            // Configurar manejo global de excepciones
            SetupGlobalExceptionHandling();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                _window = _window == null ? new MainWindow() : _window;

                // Inicializar SettingsManager primero
                await InitializeSettingsAsync();

                // Inicializar otros servicios
                await InitializeServicesAsync();

                _window.Activate();
                SplashScreen();
            }
            catch (Exception ex)
            {
                LogException(ex, "Error crítico durante el lanzamiento de la aplicación");
                
                // Intentar mostrar la ventana con página de error
                if (_window != null)
                {
                    _window.Activate();
                    ShowErrorDialog("Error de inicialización", "Ocurrió un error durante el inicio de la aplicación. Por favor, reinicie la aplicación.");
                }
                else
                {
                    Environment.Exit(1);
                }
            }
        }

        private async void SplashScreen()
        {
            try
            {
                var image = new Image();
                var imageSource = new BitmapImage(new Uri("ms-appx:///Assets/AppIcon.png"));
                image.Source = imageSource;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                image.VerticalAlignment = VerticalAlignment.Center;
                image.Width = 120;
                image.Height = 120;

                var text = new TextBlock();
                text.Text = "Server App Desktop\nVersión 1.0 Preview 2";
                text.Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"];
                text.HorizontalAlignment = HorizontalAlignment.Center;

                var ring = new ProgressRing
                {
                    IsActive = true,
                };
                ring.Width = 40;
                ring.Height = 40;

                var panel = new StackPanel();
                Grid.SetRow(panel, 1);
                panel.HorizontalAlignment = HorizontalAlignment.Center;
                panel.VerticalAlignment = VerticalAlignment.Center;
                panel.Spacing = 20;
                panel.Children.Add(image);
                panel.Children.Add(ring);
                panel.Children.Add(text);

                _window?.M_Layout.Children.Add(panel);
                _window!.M_Frame.Visibility = Visibility.Collapsed;

                await Task.Delay(1500).ConfigureAwait(true);
                panel.Children.Remove(image);
                panel.Children.Remove(text);

                ring.Width = 80;
                ring.Height = 80;
                await Task.Delay(500).ConfigureAwait(true);
                panel.Children.Remove(ring);
                _window.M_Layout.Children.Remove(panel);
                _window.M_Frame.Visibility = Visibility.Visible;

                // Navegación inteligente: verificar si es primera ejecución
                NavigateToInitialPage();
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error en SplashScreen: {ex.Message}");
#endif
                // En caso de error, navegar directamente a la página principal
                _window?.M_Frame.Navigate(typeof(BedrockMainPage));
            }
        }

        public ElementTheme CurrentRootTheme { get; private set; } = ElementTheme.Default;

        public void ApplyGlobalTheme(ElementTheme rootTheme)
        {
            CurrentRootTheme = rootTheme;

            if (_window?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = rootTheme;

                // Sincronizar colores dinámicos del caption (sin fondo sólido)
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
                var appWindow = _window.AppWindow;
                var titleBar = appWindow.TitleBar;

                if (rootTheme == ElementTheme.Dark)
                {
                    titleBar.ButtonForegroundColor = Colors.White;
                }
                else if (rootTheme == ElementTheme.Light)
                {
                    titleBar.ButtonForegroundColor = Colors.Black;
                }
                else // Default: usar heurística del sistema
                {
                    var isDark = DetectSystemThemeIsDark();
                    titleBar.ButtonForegroundColor = isDark ? Colors.White : Colors.Black;
                }

                // No se tocan los BackgroundColor para mantener fondo dinámico
            }
        }

        private static bool DetectSystemThemeIsDark()
        {
            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            var background = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
            return background == Colors.Black;
        }

        private static Windows.UI.Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Windows.UI.Color { A = a, R = r, G = g, B = b };
        }

        /// <summary>
        /// Navega a la página inicial apropiada basándose en el estado de la configuración
        /// </summary>
        private async void NavigateToInitialPage()
        {
            try
            {
                // Verificar si es primera ejecución o si la configuración no es válida
                var isFirstRun = await SettingsManager.IsFirstRunAsync();
                
                if (isFirstRun)
                {
                    // Primera ejecución: ir a configuración inicial
                    _window?.M_Frame.Navigate(typeof(InitialSettingsMainPage));
                }
                else
                {
                    // Configuración existente: cargar configuración y aplicar tema
                    var config = await SettingsManager.GetServerConfigAsync();
                    
                    if (config != null)
                    {
                        // Aplicar tema guardado
                        ElementTheme theme = config.Theme switch
                        {
                            "Claro" => ElementTheme.Light,
                            "Oscuro" => ElementTheme.Dark,
                            _ => ElementTheme.Default
                        };
                        ApplyGlobalTheme(theme);

                        // Aplicar backdrop guardado
                        ApplyBackdrop(config.Backdrop);
                    }

                    // Ir a la página principal
                    _window?.M_Frame.Navigate(typeof(BedrockMainPage));
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Error en NavigateToInitialPage");
                // En caso de error, ir a configuración inicial
                _window?.M_Frame.Navigate(typeof(InitialSettingsMainPage));
            }
        }

        /// <summary>
        /// Aplica el backdrop especificado a la ventana
        /// </summary>
        public void ApplyBackdrop(string backdropName)
        {
            try
            {
                _window!.SystemBackdrop = backdropName switch
                {
                    "Mica" => new MicaBackdrop { Kind = MicaKind.Base },
                    "Desktop Acrylic" => new DesktopAcrylicBackdrop(),
                    _ => new MicaBackdrop { Kind = MicaKind.BaseAlt } // "Mica Alt" por defecto
                };
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error aplicando backdrop: {ex.Message}");
#endif
                // Fallback a Mica Alt
                _window!.SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
            }
        }

        /// <summary>
        /// Inicializa SettingsManager de manera segura
        /// </summary>
        private async Task InitializeSettingsAsync()
        {
            try
            {
                await SettingsManager.InitializeAsync();
                Debug.WriteLine("SettingsManager inicializado correctamente");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error crítico al inicializar SettingsManager");
                throw; // Re-throw para que OnLaunched pueda manejar el error
            }
        }

        /// <summary>
        /// Inicializa los servicios de la aplicación
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            try
            {
                // Inicializar servicio de notificaciones
                await NotificationService.Instance.InitializeAsync();
                
                Debug.WriteLine("Servicios de la aplicación inicializados correctamente");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error inicializando servicios");
                // No re-throw aquí, solo log el error
            }
        }

        /// <summary>
        /// Configura el manejo global de excepciones
        /// </summary>
        private void SetupGlobalExceptionHandling()
        {
            // Manejar excepciones no controladas en el dominio de la aplicación
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                LogException(exception, "Excepción no controlada en AppDomain");

                if (e.IsTerminating)
                {
                    Debug.WriteLine("La aplicación se está cerrando debido a una excepción fatal");
                }
            };

            // Manejar excepciones no observadas en tareas
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                LogException(e.Exception, "Excepción no observada en Task");
                e.SetObserved(); // Marcar como observada para evitar que termine la aplicación
            };

            // Manejar excepciones no controladas en la UI
            Microsoft.UI.Xaml.Application.Current.UnhandledException += (sender, e) =>
            {
                LogException(e.Exception, "Excepción no controlada en UI");

                // Si la excepción es crítica, puedes decidir si cerrar la app o solo notificar
                e.Handled = true; // Marcar como manejada para evitar crash

                // Intentar mostrar un diálogo de error al usuario
                try
                {
                    ShowErrorDialog("Error de la aplicación", 
                        $"Ocurrió un error inesperado:\n{e.Exception?.Message}\n\nLa aplicación puede continuar funcionando, pero es recomendable reiniciarla.");
                }
                catch (Exception dialogEx)
                {
                    Debug.WriteLine($"No se pudo mostrar diálogo de error al usuario: {dialogEx.Message}");
                }
            };
        }

        /// <summary>
        /// Registra una excepción en el log
        /// </summary>
        private void LogException(Exception? exception, string context)
        {
            if (exception == null) return;

            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}:\n{exception}\n";
            
            Debug.WriteLine(logMessage);
            
            try
            {
                var logPath = Path.Combine(Path.GetTempPath(), "ServerAppDesktop_Errors.log");
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Si no podemos escribir al log, solo continuar
                Debug.WriteLine("No se pudo escribir al archivo de log");
            }
        }

        /// <summary>
        /// Muestra un diálogo de error al usuario
        /// </summary>
        private void ShowErrorDialog(string title, string message)
        {
            try
            {
                if (_window?.DispatcherQueue != null)
                {
                    _window.DispatcherQueue.TryEnqueue(async () =>
                    {
                        var dialog = new ContentDialog
                        {
                            Title = title,
                            Content = message,
                            CloseButtonText = "Aceptar",
                            XamlRoot = _window.Content.XamlRoot
                        };
                        
                        await dialog.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error mostrando diálogo: {ex.Message}");
            }
        }
    }
}
