namespace ServerAppDesktop;

public sealed partial class App : Application
{
    public static IHost? Host { get; private set; }
    private readonly bool _trayOnly;

    public App(bool trayOnly = false)
    {
        _trayOnly = trayOnly;
        InitializeComponent();

        if (SettingsHelper.ExistsConfigurationFile())
        {
            SettingsHelper.LoadAndSetBasicSettings();
        }
        else
        {
            DataHelper.Settings = null;
        }

        Host = AppHandler.ConfigureHost();
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        return Host!.Services.GetRequiredService<T>() ?? throw new InvalidOperationException("Host no inicializado");
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        MainWindow.Initialize();
        bool isFirstRun = DataHelper.Settings == null;

        if (!isFirstRun)
        {
            WindowHelper.SetTheme(MainWindow.Instance, DataHelper.Settings!.UI.Theme.To<ElementTheme>());
            WindowHelper.SetSystemBackdrop(MainWindow.Instance, DataHelper.Settings.UI.Backdrop);
        }

        AppInstance.FindOrRegisterForKey(DataHelper.WindowIdentifier).Activated += (_, _) =>
            MainWindow.Instance.DispatcherQueue.TryEnqueue(() => WindowHelper.ShowAndFocus(MainWindow.Instance));

        if (!_trayOnly)
        {
            WindowHelper.ShowAndFocus(MainWindow.Instance);
        }
        else
        {
            EfficiencyModeUtilities.SetEfficiencyMode(true);
        }

        if (MainWindow.Instance != null)
        {
            bool isConnected = await NetworkHelper.IsInternetAvailableAsync();
            MainWindow.Instance.ViewModel.IsConnectedToInternet = isConnected;

            if (isConnected)
            {
                await AppHandler.CheckUpdatesAsync(_trayOnly);
            }

            _ = MainWindow.Instance.contentFrame.Navigate(typeof(Page));

            _ = MainWindow.Instance.contentFrame.Navigate(
                isFirstRun ? typeof(OOBEView) : typeof(MainView),
                null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
