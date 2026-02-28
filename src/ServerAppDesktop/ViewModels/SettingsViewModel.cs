using System.Text.Json;

namespace ServerAppDesktop.ViewModels;

public sealed partial class SettingsViewModel : ObservableRecipient, IRecipient<ServerStateChangedMessage>
{
    private readonly ISettingsService _service;
    private readonly IServerPropertiesService _serverPropertiesService;
    private readonly IOOBEService _oobeService;
    private readonly MainViewModel _mainViewModel;

    private string serverFolder = "";
    private string serverExe = "";

    [ObservableProperty]
    private ObservableCollection<WindowBackdrop> _windowBackdrops = [];

    [ObservableProperty]
    private ObservableCollection<WindowTheme> _windowThemes = [];

    [ObservableProperty]
    private ObservableCollection<MinecraftEdition> _minecraftEditions = [];

    [ObservableProperty]
    private ObservableCollection<MinecraftDifficulty> _minecraftDifficulties = [];

    [ObservableProperty]
    private ObservableCollection<MinecraftGamemode> _minecraftGamemodes = [];

    [ObservableProperty]
    private ObservableCollection<Language> _languages = [];

    [ObservableProperty]
    private WindowBackdrop? selectedBackdrop;

    [ObservableProperty]
    private WindowTheme? selectedTheme;

    [ObservableProperty]
    private MinecraftEdition? selectedMinecraftEdition;

    [ObservableProperty]
    private MinecraftDifficulty? selectedMinecraftDifficulty;

    [ObservableProperty]
    private MinecraftGamemode? selectedMinecraftGamemode;

    [ObservableProperty]
    private Language? selectedLanguage;

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

    [ObservableProperty]
    private bool closeInSystemTray = true;

    [ObservableProperty]
    private int ramLimit = 1024;

    [ObservableProperty]
    private bool commandsEnabled = false;

    [ObservableProperty]
    private int maxPlayers = 10;

    [ObservableProperty]
    private bool lanVisibilty = true;

    [ObservableProperty]
    private bool _serverRunning = false;

    [ObservableProperty]
    private bool _isConfigured = false;

    [ObservableProperty]
    private bool _startWithWindows = true;

    public SettingsViewModel(MainViewModel mainViewModel, ISettingsService service, IOOBEService oobeService, IServerPropertiesService serverPropertiesService)
    {
        IsActive = true;
        _service = service;
        _serverPropertiesService = serverPropertiesService;
        _oobeService = oobeService;
        _mainViewModel = mainViewModel;

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
        MinecraftDifficulties =
            [
                new() { Name = "Pacífico", Value = "peaceful" },
                new() { Name = "Fácil", Value = "easy" },
                new() { Name = "Normal", Value = "normal" },
                new() { Name = "Difícil", Value = "hard" }
            ];
        MinecraftGamemodes =
            [
                new() { Name = "Supervivencia", Value = "survival" },
                new() { Name = "Creativo", Value = "creative" },
                new() { Name = "Aventura", Value = "adventure" }
            ];
        Languages =
            [
                new ("Sistema", ""),
                new("Español (América latina)", "es-419")
            ];

        _oobeService.OOBEFinished += (val) => IsConfigured = val;
    }

    partial void OnSelectedBackdropChanged(WindowBackdrop? value)
    {
        if (MainWindow.Instance == null || value?.Value == null)
        {
            return;
        }

        if (MainWindow.Instance.SystemBackdrop != value.Value)
        {
            MainWindow.Instance.SystemBackdrop = value.Value;
        }

        DataHelper.Settings?.UI.Backdrop = value.Index;
        SaveSettings();
    }

    partial void OnSelectedThemeChanged(WindowTheme? value)
    {
        if (MainWindow.Instance == null || value?.Value == null)
        {
            return;
        }

        WindowHelper.SetTheme(MainWindow.Instance, value.Value);
        DataHelper.Settings?.UI.Theme = value.Index;
        SaveSettings();
    }

    partial void OnServerPortChanged(int value)
    {
        if (value is 0 or > 65535)
        {
            ServerPort = SelectedMinecraftEdition?.Value == 0 ? 19132 : 25565;
        }

        _serverPropertiesService.SetValue("server-port", value);
        Messenger.Send(new ServerPropertiesChangedMessage());
    }

    partial void OnServerDescriptionChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string motdKey = (SelectedMinecraftEdition?.Value == 0) ? "server-name" : "motd";
        _serverPropertiesService.SetValue(motdKey, value);
        Messenger.Send(new ServerPropertiesChangedMessage());
    }

    partial void OnServerPathChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(serverFolder))
        {
            return;
        }

        _serverPropertiesService.SetPath(serverFolder);

        ServerPort = _serverPropertiesService.GetValue<int>("server-port");

        string motdKey = (SelectedMinecraftEdition?.Value == 0) ? "server-name" : "motd";
        ServerDescription = _serverPropertiesService.GetValue<string>(motdKey).To<string>();

        serverFolder = value;
        SelectedMinecraftGamemode = GetMinecraftGamemode();
        SelectedMinecraftDifficulty = GetMinecraftDifficulty();
        CommandsEnabled = _serverPropertiesService.GetValue<bool>("allow-cheats");
        MaxPlayers = _serverPropertiesService.GetValue<int>("max-players");
        LanVisibilty = _serverPropertiesService.GetValue<bool>("enable-lan-visibility");

        DataHelper.Settings?.Server.Path = serverFolder;
        SaveSettings();
    }

    partial void OnSelectedMinecraftEditionChanged(MinecraftEdition? value)
    {
        if (value == null)
            return;
        DataHelper.Settings?.Server.Edition = value.Value;
        SaveSettings();
    }

    partial void OnServerExecutableChanged(string value)
    {
        serverExe = value;
        DataHelper.Settings?.Server.Executable = value;
        SaveSettings();
    }

    partial void OnSelectedMinecraftDifficultyChanged(MinecraftDifficulty? value)
    {
        if (value == null)
            return;

        _serverPropertiesService.SetValue("difficulty", value.Value);
        Messenger.Send(new ServerPropertiesChangedMessage());
    }

    partial void OnSelectedMinecraftGamemodeChanged(MinecraftGamemode? value)
    {
        if (value == null)
            return;

        _serverPropertiesService.SetValue("gamemode", value.Value);
        Messenger.Send(new ServerPropertiesChangedMessage());
    }

    partial void OnLanVisibiltyChanged(bool value)
    {
        _serverPropertiesService.SetValue("enable-lan-visibility", value);
        Messenger.Send(new ServerPropertiesChangedMessage());
    }

    partial void OnCommandsEnabledChanged(bool value)
    {
        _serverPropertiesService.SetValue("allow-cheats", value);
        Messenger.Send(new ServerPropertiesChangedMessage());
    }

    partial void OnRamLimitChanged(int value)
    {
        DataHelper.Settings?.Server.RamLimit = value;
        SaveSettings();
    }

    partial void OnIsConfiguredChanged(bool value)
    {
        if (!value)
            return;

        if (DataHelper.Settings == null)
            return;

        SelectedBackdrop = WindowBackdrops[DataHelper.Settings.UI.Backdrop];
        SelectedTheme = WindowThemes[DataHelper.Settings.UI.Theme];
        SelectedMinecraftEdition = MinecraftEditions[DataHelper.Settings.Server.Edition];
        serverFolder = DataHelper.Settings.Server.Path;
        serverExe = DataHelper.Settings.Server.Executable;
        ServerPath = DataHelper.Settings.Server.Path;
        ServerExecutable = DataHelper.Settings.Server.Executable;
        AutoStartServer = DataHelper.Settings.Startup.AutoStartServer;
        CloseInSystemTray = DataHelper.Settings.Startup.CloseInSystemTray;
        RamLimit = DataHelper.Settings.Server.RamLimit;
        SelectedLanguage = Languages[GetLanguageIndex()];
    }

    private MinecraftDifficulty GetMinecraftDifficulty()
    {
        string difficulty = _serverPropertiesService.GetValue<string>("difficulty").To<string>() ?? "";

        return difficulty.ToLowerInvariant() switch
        {
            "peaceful" => MinecraftDifficulties[0],
            "easy" => MinecraftDifficulties[1],
            "normal" => MinecraftDifficulties[2],
            "hard" => MinecraftDifficulties[3],
            _ => MinecraftDifficulties[2]
        };
    }

    private MinecraftGamemode GetMinecraftGamemode()
    {
        string gamemode = _serverPropertiesService.GetValue<string>("gamemode").To<string>() ?? "";

        return gamemode.ToLowerInvariant() switch
        {
            "survival" => MinecraftGamemodes[0],
            "creative" => MinecraftGamemodes[1],
            "adventure" => MinecraftGamemodes[2],
            _ => MinecraftGamemodes[0]
        };
    }

    private int GetLanguageIndex()
    {
        string code = DataHelper.Settings?.UI.Language ?? "";

        return code.ToLowerInvariant() switch
        {
            "" => 0,
            "es-419" => 1,
            _ => 0
        };
    }

    partial void OnSelectedLanguageChanged(Language? value)
    {
        if (value == null)
            return;

        if (value.Code != (DataHelper.Settings?.UI.Language ?? ""))
        {
            DataHelper.Settings?.UI.Language = value.Code;
            SaveSettings();
            if (!string.IsNullOrEmpty(value.Code))
                ApplicationLanguages.PrimaryLanguageOverride = value.Code;
            _mainViewModel.NeedsToRestart = true;
        }
    }

    private void SaveSettings()
    {
        Messenger.Send(new AppSettingsChangedMessage(DataHelper.Settings!));
        AppSettingsJsonContext context = new(new JsonSerializerOptions
        {
            WriteIndented = true,
            IndentSize = 4
        });

        string jsonString = JsonSerializer.Serialize(DataHelper.Settings, context.AppSettings);

        string jsonFilePath = Path.Combine(DataHelper.SettingsPath, DataHelper.SettingsFile);

        if (!string.IsNullOrEmpty(DataHelper.SettingsPath) && !Directory.Exists(DataHelper.SettingsPath))
        {
            _ = Directory.CreateDirectory(DataHelper.SettingsPath);
        }

        File.WriteAllText(jsonFilePath, jsonString);
    }

    partial void OnCloseInSystemTrayChanged(bool value)
    {
        DataHelper.Settings?.Startup.CloseInSystemTray = value;
        SaveSettings();
    }

    partial void OnAutoStartServerChanged(bool value)
    {
        DataHelper.Settings?.Startup.AutoStartServer = value;
        SaveSettings();
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

    public void Receive(ServerStateChangedMessage message)
    {
        if (message?.Value is not { } state)
        {
            return;
        }

        bool isBusy = state switch
        {
            ServerStateType.Starting or ServerStateType.Stopping => true,
            ServerStateType.Running or ServerStateType.Restarting => true,
            ServerStateType.Stopped => false,
            _ => false
        };

        _ = (MainWindow.Instance?.DispatcherQueue.TryEnqueue(() =>
        {
            ServerRunning = isBusy;
        }));
    }
}
