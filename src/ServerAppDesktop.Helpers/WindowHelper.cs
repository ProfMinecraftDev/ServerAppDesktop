using System;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace ServerAppDesktop.Helpers
{
    public static class WindowHelper
    {
        public static void ShowAndFocus(Window window)
        {
            if (window == null)
                return;

            window.AppWindow.Show();
            window.Activate();
        }

        public static async Task<bool> IsRedirectedAsync()
        {
            var mainInstance = AppInstance.FindOrRegisterForKey(DataHelper.WindowIdentifier);

            if (!mainInstance.IsCurrent)
            {
                var args = AppInstance.GetCurrent().GetActivatedEventArgs();
                await mainInstance.RedirectActivationToAsync(args);
                return true;
            }
            return false;
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