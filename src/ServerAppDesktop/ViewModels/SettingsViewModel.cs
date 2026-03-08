namespace ServerAppDesktop.ViewModels;

public sealed partial class SettingsViewModel : ObservableRecipient, IRecipient<ServerStateChangedMessage>
{
    private readonly ISettingsService _settingsService;
    private readonly IServerPropertiesService _serverPropertiesService;
    private readonly IOOBEService _oobeService;
    private readonly MainViewModel _mainViewModel;

    private string serverFolder = "";
    private string serverExe = "";

    private readonly Dictionary<string, int> _diffMap = new() { ["peaceful"] = 0, ["easy"] = 1, ["normal"] = 2, ["hard"] = 3 };
    private readonly Dictionary<string, int> _gmMap = new() { ["survival"] = 0, ["creative"] = 1, ["adventure"] = 2 };
    private readonly Dictionary<string, int> _chatMap = new() { ["none"] = 0, ["dropped"] = 1, ["disabled"] = 2 };

    [ObservableProperty] private bool _serverRunning;
    [ObservableProperty] private bool _canSelectFolder = true;
    [ObservableProperty] private bool _canSelectExecutable = true;

    #region Collections (DRY: Inicialización simplificada)
    [ObservableProperty] private ObservableCollection<WindowBackdrop> _windowBackdrops = [];
    [ObservableProperty] private ObservableCollection<WindowTheme> _windowThemes = [];
    [ObservableProperty] private ObservableCollection<MinecraftEdition> _minecraftEditions = [];
    [ObservableProperty] private ObservableCollection<MinecraftDifficulty> _minecraftDifficulties = [];
    [ObservableProperty] private ObservableCollection<MinecraftGamemode> _minecraftGamemodes = [];
    [ObservableProperty] private ObservableCollection<Language> _languages = [];
    [ObservableProperty] private ObservableCollection<ChatRestriction> _chatRestrictions = [];
    #endregion

    #region Selection & Config Properties
    [ObservableProperty] private WindowBackdrop? selectedBackdrop;
    [ObservableProperty] private WindowTheme? selectedTheme;
    [ObservableProperty] private MinecraftEdition? selectedMinecraftEdition;
    [ObservableProperty] private MinecraftDifficulty? selectedMinecraftDifficulty;
    [ObservableProperty] private MinecraftGamemode? selectedMinecraftGamemode;
    [ObservableProperty] private Language? selectedLanguage;
    [ObservableProperty] private ChatRestriction? selectedChatRestriction;

    [ObservableProperty] private string serverPath = "";
    [ObservableProperty] private string serverExecutable = "";
    [ObservableProperty] private int serverPort;
    [ObservableProperty] private string serverDescription = "";
    [ObservableProperty] private bool autoStartServer;
    [ObservableProperty] private bool closeInSystemTray;
    [ObservableProperty] private int ramLimit;
    [ObservableProperty] private bool commandsEnabled;
    [ObservableProperty] private int maxPlayers;
    [ObservableProperty] private bool lanVisibilty;
    [ObservableProperty] private int viewDistance;
    [ObservableProperty] private int tickDistance;
    [ObservableProperty] private string levelName = "";
    [ObservableProperty] private bool isConfigured;
    [ObservableProperty] private bool texturePackRequired;
    [ObservableProperty] private bool playerInteraction;
    [ObservableProperty] private bool forceGamemode;
    [ObservableProperty] private bool startWithWindows;
    [ObservableProperty] private bool onlineMode;
    #endregion

    public SettingsViewModel(MainViewModel mainViewModel, ISettingsService settingsService, IOOBEService oobeService, IServerPropertiesService serverPropertiesService)
    {
        _settingsService = settingsService;
        _serverPropertiesService = serverPropertiesService;
        _oobeService = oobeService;
        _mainViewModel = mainViewModel;

        IsActive = true;

        InitializeCollections();
        _oobeService.OOBEFinished += (_, args) => IsConfigured = args.IsSuccess;
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

        MinecraftDifficulties = [new("Pacífico", "peaceful"), new("Fácil", "easy"), new("Normal", "normal"), new("Difícil", "hard")];
        MinecraftGamemodes = [new("Supervivencia", "survival"), new("Creativo", "creative"), new("Aventura", "adventure")];
        Languages = [new("Sistema", ""), new("Español (América latina)", "es-419")];
        ChatRestrictions = [new("Ninguno", "None"), new("Descartado", "Dropped"), new("Deshabilitado", "Disabled")];
    }

    #region Reducción de Lógica (DRY Methods)
    private void SaveAndNotify()
    {
        _settingsService.Save();
        Messenger.Send(new AppSettingsChangedMessage(DataHelper.Settings!));
    }

    private T GetMappedValue<T>(string key, Dictionary<string, int> map, IList<T> collection)
    {
        string val = _serverPropertiesService.GetValue<string>(key)?.ToLowerInvariant() ?? "";
        int index = map.TryGetValue(val, out int i) ? i : 0;
        return collection[index];
    }
    #endregion

    #region Partial Methods - Config Sync

    partial void OnIsConfiguredChanged(bool value)
    {
        if (!value || DataHelper.Settings == null)
            return;
        var s = DataHelper.Settings;
        SelectedBackdrop = WindowBackdrops[s.UI.Backdrop];
        SelectedTheme = WindowThemes[s.UI.Theme];
        SelectedMinecraftEdition = MinecraftEditions[s.Server.Edition];
        serverFolder = s.Server.Path;
        serverExe = s.Server.Executable;
        ServerPath = s.Server.Path;
        ServerExecutable = s.Server.Executable;
        AutoStartServer = s.Startup.AutoStartServer;
        CloseInSystemTray = s.Startup.CloseInSystemTray;
        RamLimit = s.Server.RamLimit;
        SelectedLanguage = Languages[_settingsService.GetLanguageIndex()];
        StartWithWindows = _settingsService.GetStartWithWindows();
    }

    partial void OnSelectedBackdropChanged(WindowBackdrop? value)
    {
        if (MainWindow.Instance != null && value?.Value != null)
        {
            MainWindow.Instance.SystemBackdrop = value.Value;
            DataHelper.Settings?.UI.Backdrop = value.Index;
            SaveAndNotify();
        }
    }
    partial void OnSelectedThemeChanged(WindowTheme? value)
    {
        if (MainWindow.Instance != null && value?.Value != null)
        {
            WindowHelper.SetTheme(MainWindow.Instance, value.Value);
            DataHelper.Settings?.UI.Theme = value.Index;
            SaveAndNotify();
        }
    }

    partial void OnServerPathChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(serverFolder) || !Directory.Exists(serverFolder))
            return;

        _serverPropertiesService.SetPath(serverFolder);

        bool isBedrock = SelectedMinecraftEdition?.Value == 0;

        ServerPort = _serverPropertiesService.GetValue<int>("server-port");
        ServerDescription = _serverPropertiesService.GetValue<string>(isBedrock ? "server-name" : "motd") ?? "";
        MaxPlayers = _serverPropertiesService.GetValue<int>("max-players");
        OnlineMode = _serverPropertiesService.GetValue<bool>("online-mode");
        LevelName = _serverPropertiesService.GetValue<string>("level-name") ?? "Level";

        ViewDistance = _serverPropertiesService.GetValue<int>("view-distance");
        TickDistance = _serverPropertiesService.GetValue<int>(isBedrock ? "tick-distance" : "simulation-distance");

        TexturePackRequired = _serverPropertiesService.GetValue<bool>(isBedrock ? "texturepack-required" : "require-resource-pack");
        ForceGamemode = _serverPropertiesService.GetValue<bool>("force-gamemode");

        SelectedMinecraftDifficulty = GetMappedValue("difficulty", _diffMap, MinecraftDifficulties);
        SelectedMinecraftGamemode = GetMappedValue("gamemode", _gmMap, MinecraftGamemodes);

        if (isBedrock)
        {
            SelectedChatRestriction = GetMappedValue("chat-restriction", _chatMap, ChatRestrictions);
            PlayerInteraction = !_serverPropertiesService.GetValue<bool>("disable-player-interaction");
            CommandsEnabled = _serverPropertiesService.GetValue<bool>("allow-cheats");
        }
        else
            CommandsEnabled = _serverPropertiesService.GetValue<int>("op-permission-level") >= 2;

        DataHelper.Settings!.Server.Path = serverFolder;
        SaveAndNotify();
    }

    partial void OnSelectedMinecraftEditionChanged(MinecraftEdition? value)
    {
        if (value == null)
            return;
        DataHelper.Settings!.Server.Edition = value.Value;
        SaveAndNotify();
    }

    partial void OnServerExecutableChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(serverExe))
            return;
        DataHelper.Settings!.Server.Executable = serverExe;
        SaveAndNotify();
    }

    partial void OnAutoStartServerChanged(bool value)
    {
        DataHelper.Settings!.Startup.AutoStartServer = value;
        SaveAndNotify();
    }

    partial void OnCloseInSystemTrayChanged(bool value)
    {
        DataHelper.Settings!.Startup.CloseInSystemTray = value;
        SaveAndNotify();
    }

    partial void OnRamLimitChanged(int value)
    {
        DataHelper.Settings!.Server.RamLimit = value;
        SaveAndNotify();
    }

    partial void OnSelectedLanguageChanged(Language? value)
    {
        if (value == null)
            return;
        if (value.Code != DataHelper.Settings!.UI.Language)
        {
            DataHelper.Settings!.UI.Language = value.Code;
            _mainViewModel.NeedsToRestart = true;
        }

        SaveAndNotify();
    }

    partial void OnStartWithWindowsChanged(bool value) => _settingsService.SetStartWithWindows(value);

    #endregion

    #region Partial Methods - Server Properties
    partial void OnServerPortChanged(int value) => _serverPropertiesService.SetValue("server-port", value);

    partial void OnServerDescriptionChanged(string value)
    {
        string key = SelectedMinecraftEdition?.Value == 0 ? "server-name" : "motd";
        _serverPropertiesService.SetValue(key, value);
    }

    partial void OnMaxPlayersChanged(int value) => _serverPropertiesService.SetValue("max-players", value);

    partial void OnOnlineModeChanged(bool value) => _serverPropertiesService.SetValue("online-mode", value);

    partial void OnViewDistanceChanged(int value) => _serverPropertiesService.SetValue("view-distance", value);

    partial void OnTickDistanceChanged(int value)
    {
        string key = SelectedMinecraftEdition?.Value == 0 ? "tick-distance" : "simulation-distance";
        _serverPropertiesService.SetValue(key, value);
    }

    partial void OnLevelNameChanged(string value) => _serverPropertiesService.SetValue("level-name", value);

    partial void OnTexturePackRequiredChanged(bool value)
    {
        string key = SelectedMinecraftEdition?.Value == 0 ? "texturepack-required" : "require-resource-pack";
        _serverPropertiesService.SetValue(key, value);
    }

    partial void OnForceGamemodeChanged(bool value) => _serverPropertiesService.SetValue("force-gamemode", value);

    partial void OnLanVisibiltyChanged(bool value) => _serverPropertiesService.SetValue("enable-lan-visibility", value);

    partial void OnSelectedMinecraftDifficultyChanged(MinecraftDifficulty? value)
    {
        if (value != null)
            _serverPropertiesService.SetValue("difficulty", value.Value);
    }

    partial void OnSelectedMinecraftGamemodeChanged(MinecraftGamemode? value)
    {
        if (value != null)
            _serverPropertiesService.SetValue("gamemode", value.Value);
    }

    partial void OnSelectedChatRestrictionChanged(ChatRestriction? value)
    {
        if (SelectedMinecraftEdition?.Value == 0 && value != null)
            _serverPropertiesService.SetValue("chat-restriction", value.Value);
    }

    partial void OnPlayerInteractionChanged(bool value)
    {
        if (SelectedMinecraftEdition?.Value == 0)
            _serverPropertiesService.SetValue("disable-player-interaction", !value);
    }

    partial void OnCommandsEnabledChanged(bool value)
    {
        bool isBedrock = SelectedMinecraftEdition?.Value == 0;

        if (isBedrock)
        {
            _serverPropertiesService.SetValue("allow-cheats", value);
        }
        else
        {
            _serverPropertiesService.SetValue("op-permission-level", value ? 2 : 1);

            _serverPropertiesService.SetValue("enable-command-block", value);
        }
    }

    #endregion

    #region Picker Logic (Generic & DRY)

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

        TResult result = await pickAction(picker);

        if (result is PickFileResult pfr)
            onResult(pfr.Path);
        else if (result is PickFolderResult pfor)
            onResult(pfor.Path);
    }

    #endregion

    public void Receive(ServerStateChangedMessage message)
    {
        bool isBusy = message.Value is ServerStateType.Starting or ServerStateType.Stopping or ServerStateType.Running or ServerStateType.Restarting;
        MainWindow.Instance?.DispatcherQueue.TryEnqueue(() => ServerRunning = isBusy);
    }
}
