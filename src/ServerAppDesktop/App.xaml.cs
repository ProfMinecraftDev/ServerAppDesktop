using Microsoft.UI.Xaml;

namespace ServerAppDesktop
{
    public sealed partial class App : Application
    {
        private MainWindow? MainWindow;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
