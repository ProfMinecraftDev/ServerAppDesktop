namespace ServerAppDesktop.ViewModels;

public sealed partial class OOBEViewModel : ObservableObject
{
    private readonly IOOBEService _service;
    private readonly HomeViewModel _homeViewModel;
    private readonly IServerPropertiesService _serverPropertiesService;

    private string serverFolder = "";
    private string serverExe = "";

    [ObservableProperty] private ObservableCollection<WindowBackdrop> _windowBackdrops = [];
    [ObservableProperty] private ObservableCollection<WindowTheme> _windowThemes = [];
    [ObservableProperty] private ObservableCollection<MinecraftEdition> _minecraftEditions = [];

    [ObservableProperty] private WindowBackdrop? selectedBackdrop;
    [ObservableProperty] private WindowTheme? selectedTheme;
    [ObservableProperty] private MinecraftEdition? selectedMinecraftEdition;

    [ObservableProperty] private string serverPath = "";
    [ObservableProperty] private string serverExecutable = "";
    [ObservableProperty] private int serverPort = 0;
    [ObservableProperty] private string serverDescription = "";
    [ObservableProperty] private bool autoStartServer = true;
    [ObservableProperty] private bool canSelectFolder = true;
    [ObservableProperty] private bool canSelectExecutable = true;

    public OOBEViewModel(IOOBEService service, HomeViewModel homeViewModel, IServerPropertiesService serverPropertiesService)
    {
        (_service, _homeViewModel, _serverPropertiesService) = (service, homeViewModel, serverPropertiesService);
        _homeViewModel.IsConfigured = false;

        InitializeCollections();

        _service.OOBEFinished += (_, args) =>
        {
            if (args.IsSuccess)
                MainWindow.Instance.contentFrame.Navigate(typeof(MainView), null, new DrillInNavigationTransitionInfo());
        };
    }

    private void InitializeCollections()
    {
        WindowBackdrops = [
            new() { Name = ResourceHelper.GetString("MicaBackdropItem"), Value = new MicaBackdrop { Kind = MicaKind.Base }, Index = 0 },
            new() { Name = ResourceHelper.GetString("MicaAltBackdropItem"), Value = new MicaBackdrop { Kind = MicaKind.BaseAlt }, Index = 1 },
            new() { Name = ResourceHelper.GetString("DesktopAcrylicBackdropItem"), Value = new DesktopAcrylicBackdrop(), Index = 2 }
        ];
        WindowThemes = [
            new() { Name = ResourceHelper.GetString("SystemThemeItem"), Value = ElementTheme.Default, Index = 0 },
            new() { Name = ResourceHelper.GetString("LightThemeItem"), Value = ElementTheme.Light, Index = 1 },
            new() { Name = ResourceHelper.GetString("DarkThemeItem"), Value = ElementTheme.Dark, Index = 2 }
        ];
        MinecraftEditions = [
            new() { Name = ResourceHelper.GetString("BedrockEditionItem"), Value = 0 },
            new() { Name = ResourceHelper.GetString("JavaEditionItem"), Value = 1 }
        ];

        (SelectedBackdrop, SelectedTheme, SelectedMinecraftEdition) = (WindowBackdrops[0], WindowThemes[0], MinecraftEditions[0]);
    }

    partial void OnSelectedBackdropChanged(WindowBackdrop? value) { if (MainWindow.Instance != null && value?.Value != null) MainWindow.Instance.SystemBackdrop = value.Value; }
    partial void OnSelectedThemeChanged(WindowTheme? value) { if (MainWindow.Instance != null && value?.Value != null) WindowHelper.SetTheme(MainWindow.Instance, value.Value); }

    partial void OnServerPortChanged(int value)
    {
        if (value is <= 0 or > 65535)
            ServerPort = SelectedMinecraftEdition?.Value == 0 ? 19132 : 25565;
        _serverPropertiesService.SetValue("server-port", ServerPort);
    }

    partial void OnServerDescriptionChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        _serverPropertiesService.SetValue(SelectedMinecraftEdition?.Value == 0 ? "server-name" : "motd", value);
    }

    partial void OnServerPathChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        _serverPropertiesService.SetPath(value);
        ServerPort = _serverPropertiesService.GetValue<int>("server-port");
        ServerDescription = _serverPropertiesService.GetValue<string>(SelectedMinecraftEdition?.Value == 0 ? "server-name" : "motd") ?? ResourceHelper.GetString("OOBE_Default_MOTD");
    }

    [RelayCommand]
    private async Task SelectServerPathAsync()
    {
        nint hWnd = MainWindow.Instance.GetWindowHandle();
        WindowId mainWindowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        CanSelectFolder = false;
        await SelectLocationAsync(
            new FolderPicker(mainWindowId),
            p => p.PickSingleFolderAsync(),
            path =>
            {
                serverFolder = path;
                ServerPath = path;
            }
        );
        CanSelectFolder = true;
    }

    [RelayCommand]
    private async Task SelectServerExecutableAsync()
    {
        nint hWnd = MainWindow.Instance.GetWindowHandle();
        WindowId mainWindowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        CanSelectExecutable = false;
        await SelectLocationAsync(
            new FileOpenPicker(mainWindowId),
            p => p.PickSingleFileAsync(),
            path =>
            {
                serverExe = path;
                ServerExecutable = path;
            },
            filters: [SelectedMinecraftEdition?.Value == 0 ? ".exe" : ".jar"]
        );
        CanSelectExecutable = true;
    }

    private static async Task SelectLocationAsync<TPicker, TResult>(
    TPicker picker,
    Func<TPicker, IAsyncOperation<TResult?>> pickAction,
    Action<string> onResult,
    string[]? filters = null) where TPicker : class
    {
        if (picker is FolderPicker fp)
        {
            fp.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            fp.ViewMode = PickerViewMode.List;
        }
        else if (picker is FileOpenPicker fop && filters != null)
        {
            fop.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            foreach (string f in filters)
                fop.FileTypeFilter.Add(f);
        }

        TResult? result = await pickAction(picker);

        if (result is PickFileResult pfr)
            onResult(pfr.Path);
        else if (result is PickFolderResult pfor)
            onResult(pfor.Path);
    }

    [RelayCommand]
    private void SaveOOBESettings()
    {
        if (string.IsNullOrEmpty(serverExe) || string.IsNullOrEmpty(serverFolder))
        {
            ShowMsg(ResourceHelper.GetString("OOBE_Error_Title"), ResourceHelper.GetString("OOBE_Error_Msg"), MESSAGEBOX_STYLE.MB_ICONERROR);
            return;
        }

        var settings = new AppSettings
        {
            UI = new() { Backdrop = SelectedBackdrop?.Index ?? 0, Theme = SelectedTheme?.Index ?? 0 },
            Server = new() { Path = serverFolder, Executable = serverExe, Edition = SelectedMinecraftEdition?.Value ?? 0 },
            Startup = new() { AutoStartServer = AutoStartServer }
        };

        OnServerDescriptionChanged(ServerDescription);
        OnServerPortChanged(ServerPort);

        if (settings.Server.Edition == 1)
            ShowMsg(ResourceHelper.GetString("OOBE_Java_Title"), ResourceHelper.GetString("OOBE_Java_Msg"), MESSAGEBOX_STYLE.MB_ICONINFORMATION);

        _service.SaveUserSettings(settings);
        _service.FinishOOBE();
    }

    private static void ShowMsg(string title, string msg, MESSAGEBOX_STYLE style) => PInvoke.MessageBox(new(MainWindow.Instance.GetWindowHandle()), msg, title, style | MESSAGEBOX_STYLE.MB_SETFOREGROUND | MESSAGEBOX_STYLE.MB_APPLMODAL);
}
