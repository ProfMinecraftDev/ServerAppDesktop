using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Threading.Tasks;

namespace ServerAppDesktop.Services
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public sealed class NotificationService
    {
        private static NotificationService? _instance;
        private static readonly object _lock = new object();
        private bool _isInitialized = false;

        public static NotificationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new NotificationService();
                        }
                    }
                }
                return _instance;
            }
        }

        private NotificationService()
        {
        }

        public Task InitializeAsync()
        {
            if (_isInitialized) return Task.CompletedTask;

            try
            {
                // Registrar la aplicación para notificaciones
                AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
                AppNotificationManager.Default.Register();
                
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("NotificationService inicializado correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando NotificationService: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            // Manejar acciones de notificación si es necesario
            System.Diagnostics.Debug.WriteLine($"Notificación activada: {args.Argument}");
        }

        public async Task ShowNotificationAsync(string title, string message, NotificationType type = NotificationType.Info)
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            try
            {
                var builder = new AppNotificationBuilder()
                    .AddText(title)
                    .AddText(message);

                // Configurar icono y sonido según el tipo
                switch (type)
                {
                    case NotificationType.Success:
                        builder.SetScenario(AppNotificationScenario.Default);
                        break;
                    case NotificationType.Warning:
                        builder.SetScenario(AppNotificationScenario.Urgent);
                        break;
                    case NotificationType.Error:
                        builder.SetScenario(AppNotificationScenario.Urgent);
                        break;
                    default:
                        builder.SetScenario(AppNotificationScenario.Default);
                        break;
                }

                var notification = builder.BuildNotification();
                AppNotificationManager.Default.Show(notification);

                System.Diagnostics.Debug.WriteLine($"Notificación mostrada: {title} - {message}");
            }
            catch (Exception ex)
            {
                // Fallback a debug si falla la notificación
                System.Diagnostics.Debug.WriteLine($"Error mostrando notificación: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Notificación fallback: [{type}] {title}: {message}");
            }
        }

        public async Task ShowServerStatusAsync(string serverStatus, bool isError = false)
        {
            var type = isError ? NotificationType.Error : NotificationType.Info;
            await ShowNotificationAsync("Estado del Servidor", serverStatus, type);
        }

        public async Task ShowConfigurationMessageAsync(string message, bool isError = false)
        {
            var type = isError ? NotificationType.Error : NotificationType.Success;
            var title = isError ? "Error de Configuración" : "Configuración";
            await ShowNotificationAsync(title, message, type);
        }

        public async Task ShowSuccessAsync(string title, string message)
        {
            await ShowNotificationAsync(title, message, NotificationType.Success);
        }

        public async Task ShowErrorAsync(string title, string message)
        {
            await ShowNotificationAsync(title, message, NotificationType.Error);
        }

        public async Task ShowWarningAsync(string title, string message)
        {
            await ShowNotificationAsync(title, message, NotificationType.Warning);
        }

        public async Task ShowInfoAsync(string title, string message)
        {
            await ShowNotificationAsync(title, message, NotificationType.Info);
        }

        public void Dispose()
        {
            if (_isInitialized)
            {
                try
                {
                    AppNotificationManager.Default.NotificationInvoked -= OnNotificationInvoked;
                    AppNotificationManager.Default.Unregister();
                    _isInitialized = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al hacer dispose de NotificationService: {ex.Message}");
                }
            }
        }
    }

    // Extensiones para facilitar el uso desde otras clases
    public static class NotificationExtensions
    {
        public static async Task NotifySuccessAsync(this object source, string message)
        {
            await NotificationService.Instance.ShowSuccessAsync("Éxito", message);
        }

        public static async Task NotifyErrorAsync(this object source, string message)
        {
            await NotificationService.Instance.ShowErrorAsync("Error", message);
        }

        public static async Task NotifyInfoAsync(this object source, string message)
        {
            await NotificationService.Instance.ShowInfoAsync("Información", message);
        }

        public static async Task NotifyWarningAsync(this object source, string message)
        {
            await NotificationService.Instance.ShowWarningAsync("Advertencia", message);
        }

        public static async Task NotifyServerStatusAsync(this object source, string status, bool isError = false)
        {
            await NotificationService.Instance.ShowServerStatusAsync(status, isError);
        }
    }
}
