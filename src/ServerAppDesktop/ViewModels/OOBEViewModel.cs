

namespace ServerAppDesktop.ViewModels;

public partial class OOBEViewModel : ObservableObject
{
    private readonly IOOBEService _service;
    private readonly HomeViewModel _homeViewModel;
    private readonly IServerPropertiesService _serverPropertiesService;

    private string serverFolder = "";
    private string serverExe = "";

    [ObservableProperty]
    private ObservableCollection<WindowBackdrop> _windowBackdrops = [];

    [ObservableProperty]
    private ObservableCollection<WindowTheme> _windowThemes = [];

    [ObservableProperty]
    private ObservableCollection<MinecraftEdition> _minecraftEditions = [];

    [ObservableProperty]
    private WindowBackdrop? selectedBackdrop;

    [ObservableProperty]
    private WindowTheme? selectedTheme;

    [ObservableProperty]
    private MinecraftEdition? selectedMinecraftEdition;

    [ObservableProperty]
    private string serverPath = "";

    [ObservableProperty]
    private bool canSelectFolder = true;

    [ObservableProperty]
    private string serverExecutable = "";

    [ObservableProperty]
    private bool canSelectExecutable = true;

    [ObservableProperty]
    private int serverPort = 0;

    [ObservableProperty]
    private string serverDescription = "";

    [ObservableProperty]
    private bool autoStartServer = true;

    public OOBEViewModel(IOOBEService service, HomeViewModel homeViewModel, IServerPropertiesService serverPropertiesService)
    {
        _service = service;
        _homeViewModel = homeViewModel;
        _homeViewModel.IsConfigured = false;
        _serverPropertiesService = serverPropertiesService;

        _service.OOBEFinished += (configured) =>
         {
             if (configured)
             {
                 _ = MainWindow.Instance.contentFrame.Navigate(typeof(MainView), null, new DrillInNavigationTransitionInfo());
             }
         };

        WindowBackdrops =
            [
                new() { Name = ResourceHelper.GetString("MicaBackdropItem"), Value = new MicaBackdrop { Kind = MicaKind.Base }, Index = 0 },
                new() { Name = ResourceHelper.GetString("MicaAltBackdropItem"), Value = new MicaBackdrop { Kind = MicaKind.BaseAlt }, Index = 1 },
                new() { Name = ResourceHelper.GetString("DesktopAcrylicBackdropItem"), Value = new DesktopAcrylicBackdrop(), Index = 2 },
            ];
        WindowThemes =
            [
                new() { Name = ResourceHelper.GetString("SystemThemeItem"), Value = ElementTheme.Default, Index = 0 },
                new() { Name = ResourceHelper.GetString("LightThemeItem"), Value = ElementTheme.Light, Index = 1 },
                new() { Name = ResourceHelper.GetString("DarkThemeItem"), Value = ElementTheme.Dark, Index = 2 }
            ];
        MinecraftEditions =
            [
                new() { Name = ResourceHelper.GetString("BedrockEditionItem"), Value = 0 },
                new() { Name = ResourceHelper.GetString("JavaEditionItem"), Value = 1 }
            ];

        SelectedBackdrop = WindowBackdrops[0];
        SelectedTheme = WindowThemes[0];
        SelectedMinecraftEdition = MinecraftEditions[0];
    }

    partial void OnSelectedBackdropChanged(WindowBackdrop? value)
    {
        if (MainWindow.Instance == null || value?.Value == null)
            return;

        if (MainWindow.Instance.SystemBackdrop != value.Value)
            MainWindow.Instance.SystemBackdrop = value.Value;
    }

    partial void OnSelectedThemeChanged(WindowTheme? value)
    {
        if (MainWindow.Instance == null || value?.Value == null)
            return;

        WindowHelper.SetTheme(MainWindow.Instance, value.Value);
    }

    partial void OnServerPortChanged(int value)
    {
        if (value == 0 || value > 65535)
            ServerPort = SelectedMinecraftEdition?.Value == 0 ? 19132 : 25565;

        _serverPropertiesService.SetValue("server-port", value);
    }

    partial void OnServerDescriptionChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        string motdKey = (SelectedMinecraftEdition?.Value == 0) ? "server-name" : "motd";
        _serverPropertiesService.SetValue(motdKey, value);
    }

    partial void OnServerPathChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(serverFolder))
            return;

        _serverPropertiesService.SetPath(serverFolder);

        ServerPort = _serverPropertiesService.GetValue<int>("server-port");

        string motdKey = (SelectedMinecraftEdition?.Value == 0) ? "server-name" : "motd";
        ServerDescription = _serverPropertiesService.GetValue<string>(motdKey) ?? "A Minecraft Server";
    }

    [RelayCommand]
    private async Task SelectServerPathAsync(XamlRoot xamlRoot)
    {
        CanSelectFolder = false;

        FolderPicker picker = new(xamlRoot.ContentIslandEnvironment.AppWindowId)
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            ViewMode = PickerViewMode.List
        };

        PickFolderResult? folder = await picker.PickSingleFolderAsync();
        serverFolder = folder != null
            ? folder.Path
            : "";
        ServerPath = folder != null
            ? folder.Path
            : ResourceHelper.GetString("NoPathSelectedText");

        CanSelectFolder = true;
    }

    [RelayCommand]
    private async Task SelectServerExecutableAsync(XamlRoot xamlRoot)
    {
        CanSelectExecutable = false;

        FileOpenPicker picker = new(xamlRoot.ContentIslandEnvironment.AppWindowId)
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            ViewMode = PickerViewMode.List
        };

        picker.FileTypeFilter.Add(SelectedMinecraftEdition?.Value == 0 ? ".exe" : ".jar");

        PickFileResult? file = await picker.PickSingleFileAsync();
        serverExe = file != null
            ? file.Path
            : "";
        ServerExecutable = file != null
            ? file.Path
            : ResourceHelper.GetString("NoPathSelectedText");

        CanSelectExecutable = true;

    }

    [RelayCommand]
    private void SaveOOBESettings()
    {
        if (serverExe == "" || serverFolder == "")
        {
            HWND hWnd = new(MainWindow.Instance.GetWindowHandle());

            _ = PInvoke.MessageBox(
                hWnd,
                "Selecciona una ruta al servidor o ejecutable (necesita ser obligatorio)",
                "Error",
                MESSAGEBOX_STYLE.MB_ICONERROR |
                MESSAGEBOX_STYLE.MB_OK |
                MESSAGEBOX_STYLE.MB_SETFOREGROUND |
                MESSAGEBOX_STYLE.MB_APPLMODAL);

            return;
        }
        AppSettings userSettings = new()
        {
            UI = new UISettings
            {
                Backdrop = SelectedBackdrop?.Index ?? 0,
                Theme = SelectedTheme?.Index ?? 0,
            },
            Server = new ServerSettings
            {
                Path = serverFolder,
                Executable = serverExe,
                Edition = SelectedMinecraftEdition?.Value ?? 0
            },
            Startup = new StartupSettings
            {
                AutoStartServer = AutoStartServer
            }
        };
        OnServerDescriptionChanged(ServerDescription);
        OnServerPortChanged(ServerPort);
        if (userSettings.Server.Edition == 1)
        {
            HWND hWnd = new(MainWindow.Instance.GetWindowHandle());

            _ = PInvoke.MessageBox(
                hWnd,
                "Seleccionaste Minecraft Java. Asegúrate de tener Java instalado y en el PATH para evitar errores.",
                "Instalación de Java recomendada",
                MESSAGEBOX_STYLE.MB_ICONINFORMATION |
                MESSAGEBOX_STYLE.MB_OK |
                MESSAGEBOX_STYLE.MB_SETFOREGROUND |
                MESSAGEBOX_STYLE.MB_APPLMODAL);
        }
        _service.SaveUserSettings(userSettings);
        _service.FinishOOBE();
    }
}
