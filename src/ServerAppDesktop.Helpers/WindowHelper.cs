using System;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Microsoft.Windows.AppLifecycle;

namespace ServerAppDesktop.Helpers
{
    public static class WindowHelper
    {
        public static void ShowAndFocus(AppWindow appWindow)
        {
            if (appWindow == null)
                return;

            appWindow.Show();

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = true;
                presenter.IsAlwaysOnTop = false; // Quitar el "siempre encima" inmediatamente
            }
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
    }
}