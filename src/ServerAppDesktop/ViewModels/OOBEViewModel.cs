using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.Storage.Pickers;
using ServerAppDesktop.Helpers;
using ServerAppDesktop.Models;
using ServerAppDesktop.Services;

namespace ServerAppDesktop.ViewModels
{
    public partial class OOBEViewModel : ObservableObject
    {
        private readonly IOOBEService _service;

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

        public OOBEViewModel(IOOBEService _service)
        {
            this._service = _service;

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
            if (MainWindow.Current == null || value?.Value == null)
                return;

            if (MainWindow.Current.SystemBackdrop != value.Value)
                MainWindow.Current.SystemBackdrop = value.Value;
        }

        partial void OnSelectedThemeChanged(WindowTheme? value)
        {
            if (MainWindow.Current == null || value?.Value == null)
                return;

            WindowHelper.SetTheme(MainWindow.Current, value.Value);
        }

        partial void OnServerPortChanged(int value)
        {
            if (value == 0 || value > 65535)
                ServerPort = SelectedMinecraftEdition?.Value == 0 ? 19132 : 25565;
        }

        [RelayCommand]
        private async Task SelectServerPathAsync(XamlRoot xamlRoot)
        {
            CanSelectFolder = false;

            var picker = new FolderPicker(xamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.List
            };

            var folder = await picker.PickSingleFolderAsync();
            ServerPath = folder != null
                ? folder.Path
                : ResourceHelper.GetString("NoPathSelectedText");

            CanSelectFolder = true;
        }

        [RelayCommand]
        private async Task SelectServerExecutableAsync(XamlRoot xamlRoot)
        {
            CanSelectExecutable = false;

            var picker = new FileOpenPicker(xamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.List
            };

            picker.FileTypeFilter.Add(".exe");
            picker.FileTypeFilter.Add(".jar");

            var file = await picker.PickSingleFileAsync();
            ServerExecutable = file != null
                ? file.Path
                : ResourceHelper.GetString("NoPathSelectedText");

            CanSelectExecutable = true;

        }

        [RelayCommand]
        private void SaveOOBESettings()
        {
            var userSettings = new AppSettings
            {
                UI = new UISettings
                {
                    Backdrop = SelectedBackdrop?.Index ?? 0,
                    Theme = SelectedTheme?.Index ?? 0,
                },
                Server = new ServerSettings
                {
                    Path = ServerPath,
                    Executable = ServerExecutable,
                    Edition = SelectedMinecraftEdition?.Value ?? 0
                },
                Startup = new StartupSettings
                {
                    AutoStartServer = AutoStartServer
                }
            };
            _service.SaveUserSettings(userSettings);
            _service.RestartApplication();
        }
    }
}
