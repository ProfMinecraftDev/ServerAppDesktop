namespace ServerAppDesktop;

public sealed partial class MainWindow : WindowEx
{
    private static MainWindow? _instance;
    private static bool _isInitialized = false;
    private readonly IWindowHandler _windowHandler;
    private readonly HomeViewModel _homeViewModel;
    private readonly FilesViewModel _filesViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly INavigationService _navigationService;

    public static MainWindow Instance => _instance ?? throw new InvalidOperationException(ResourceHelper.GetString("Err_MainWindowNotInitialized"));
    public bool IsMouseOverTitleBar { get; set; } = false;
    public static void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        if (_instance == null)
        {
            _instance = new MainWindow();
            _isInitialized = true;
        }
    }

    public MainViewModel ViewModel { get; }

    private MainWindow()
    {
        InitializeComponent();

        _windowHandler = App.GetRequiredService<IWindowHandler>();
        _windowHandler.SetWindow(this);
        _windowHandler.LoadPersistence();
        ViewModel = App.GetRequiredService<MainViewModel>();
        _homeViewModel = App.GetRequiredService<HomeViewModel>();
        _navigationService = App.GetRequiredService<INavigationService>();
        _settingsViewModel = App.GetRequiredService<SettingsViewModel>();
        _filesViewModel = App.GetRequiredService<FilesViewModel>();

        PersistenceId = DataHelper.WindowPersistenceID;

        if (!File.Exists(Path.Join(DataHelper.SettingsPath, DataHelper.WindowPersistenceFile)))
            this.CenterOnScreen();

        TitleBar.PointerEntered += (_, _) =>
        {
            IsMouseOverTitleBar = true;
            try
            {
                if (!_windowHandler.WindowClosed && PresenterKind == AppWindowPresenterKind.FullScreen)
                {
                    TitleBar.Height = 48;
                }
            }
            catch { }
        };

        TitleBar.PointerExited += async (_, _) =>
        {
            IsMouseOverTitleBar = false;
            try
            {
                if (!_windowHandler.WindowClosed && PresenterKind == AppWindowPresenterKind.FullScreen)
                {
                    await Task.Delay(1000);
                    if (!IsMouseOverTitleBar && PresenterKind == AppWindowPresenterKind.FullScreen)
                    {
                        TitleBar.Height = 2;
                    }
                }
            }
            catch { }
        };

        NetworkHelper.ConnectionChanged += (s) =>
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                if (!(ViewModel.IsConnectedToInternet && s.As<bool>()))
                {
                    ViewModel.IsConnectedToInternet = s.As<bool>();
                    _windowHandler.HandleNetworkUIUpdate(internetInfoBar, s.As<bool>());
                }
            });
        };

        if (ObjectExtensions.As<Grid>(Content) is Grid grid)
        {
            grid.Loaded += (_, _) => _homeViewModel.IsConfigured = DataHelper.Settings != null;
            grid.Loaded += (_, _) => _settingsViewModel.IsConfigured = DataHelper.Settings != null;
            grid.Loaded += (_, _) => _filesViewModel.IsConfigured = DataHelper.Settings != null;
            grid.DataContext = ViewModel;
            grid.KeyDown += (_, e) => OnF11OrEscapeInvoked(e.Key);
        }
        fullScreenButton.Click += (_, _) => OnF11OrEscapeInvoked(VirtualKey.F11);

        _windowHandler.Configure();
        _ = _windowHandler.UpdateFullScreenLogic(false, fullScreenButton);
    }

    private void OnF11OrEscapeInvoked(VirtualKey vKey)
    {
        bool isFullScreen = PresenterKind == AppWindowPresenterKind.FullScreen;
        if (vKey == VirtualKey.F11)
        {
            _ = _windowHandler.UpdateFullScreenLogic(!isFullScreen, fullScreenButton);
        }
        else if (vKey == VirtualKey.Escape && isFullScreen)
        {
            _ = _windowHandler.UpdateFullScreenLogic(false, fullScreenButton);
        }
    }

    private void TitleBar_BackRequested(Microsoft.UI.Xaml.Controls.TitleBar _, object __)
    {
        if (ViewModel.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }
}
