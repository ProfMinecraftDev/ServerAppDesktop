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

namespace ServerAppDesktop.ViewModels
{
    public partial class OOBEViewModel : ObservableObject
    {
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

        public OOBEViewModel()
        {
            WindowBackdrops =
                [
                    new() { Name = ResourceHelper.GetString("MicaBackdropItem"), Value = new MicaBackdrop { Kind = MicaKind.Base } },
                    new() { Name = ResourceHelper.GetString("MicaAltBackdropItem"), Value = new MicaBackdrop { Kind = MicaKind.BaseAlt } },
                    new() { Name = ResourceHelper.GetString("DesktopAcrylicBackdropItem"), Value = new DesktopAcrylicBackdrop() },
                ];
            WindowThemes =
                [
                    new() { Name = ResourceHelper.GetString("SystemThemeItem"), Value = ElementTheme.Default },
                    new() { Name = ResourceHelper.GetString("LightThemeItem"), Value = ElementTheme.Light },
                    new() { Name = ResourceHelper.GetString("DarkThemeItem"), Value = ElementTheme.Dark }
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
            if (App.MainWindow == null || value?.Value == null)
                return;

            App.MainWindow.SystemBackdrop = value.Value;
        }

        partial void OnSelectedThemeChanged(WindowTheme? value)
        {
            if (App.MainWindow == null || value?.Value == null)
                return;

            WindowHelper.SetTheme(App.MainWindow, value.Value);
        }

        [RelayCommand]
        private async Task SelectServerPathAsync(XamlRoot xamlRoot)
        {
            //disable the button to avoid double-clicking
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
    }
}
