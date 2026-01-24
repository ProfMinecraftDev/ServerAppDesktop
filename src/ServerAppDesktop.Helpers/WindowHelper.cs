using System;
using System.Threading.Tasks;
using H.NotifyIcon;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinRT.Interop;

namespace ServerAppDesktop.Helpers
{
    public static class WindowHelper
    {
        public static void ShowAndFocus(Window window)
        {
            if (window == null)
                return;

            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            PInvoke.SetForegroundWindow(new HWND(hWnd));
            WindowExtensions.Show(window, true);
            window.Activate();
        }

        public static async Task<bool> IsRedirectedAsync()
        {
            // Obtenemos cómo se activó esta instancia
            var args = AppInstance.GetCurrent().GetActivatedEventArgs();

            // Si la activación es por notificación, Windows App SDK a veces 
            // crea un flujo distinto. Forzamos la búsqueda de la instancia vieja.
            var existingInstance = AppInstance.FindOrRegisterForKey(DataHelper.WindowIdentifier);

            if (!existingInstance.IsCurrent)
            {
                // Redirigimos los argumentos (incluyendo el "action=activate" que pusimos arriba)
                await existingInstance.RedirectActivationToAsync(args);
                return true;
            }

            return false;
        }

        public static void RegisterAUMID()
        {
            PInvoke.SetCurrentProcessExplicitAppUserModelID(DataHelper.WindowIdentifier);
        }

        public static void SetTheme(Window window, ElementTheme elementTheme)
        {
            var content = (FrameworkElement)window.Content;
            switch (elementTheme)
            {
                case ElementTheme.Default:
                    content.RequestedTheme = elementTheme;
                    window.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
                    break;

                case ElementTheme.Light:
                    content.RequestedTheme = elementTheme;
                    window.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.Light;
                    break;

                case ElementTheme.Dark:
                    content.RequestedTheme = elementTheme;
                    window.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.Dark;
                    break;
            }
        }
    }
}