namespace ServerAppDesktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
public sealed partial class FilesViewModel : ObservableRecipient, IRecipient<AppSettingsChangedMessage>
{

    [ObservableProperty]
    private bool _isConfigured = false;

    [ObservableProperty]
    private string _diskLabel = "";

    [ObservableProperty]
    private string _storageSizeInfo = "";

    [ObservableProperty]
    private double _storageValue = 0;

    public ObservableCollection<ServerFile> ServerFiles { get; } = [];

    public FilesViewModel()
    {
        IsActive = true;
    }

    partial void OnIsConfiguredChanged(bool value)
    {
        var path = DataHelper.Settings?.Server?.Path;
        OnPathChanged(path);
    }

    private void OnPathChanged(string? path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                string? driveRoot = Path.GetPathRoot(path);

                if (!string.IsNullOrEmpty(driveRoot))
                {
                    var info = new DriveInfo(driveRoot);

                    if (info.IsReady)
                    {
                        double totalBytes = info.TotalSize;
                        double freeBytes = info.AvailableFreeSpace;
                        double usedBytes = totalBytes - freeBytes;

                        DiskLabel = $"{(string.IsNullOrEmpty(info.VolumeLabel) ? "" : info.VolumeLabel)} ({info.Name.TrimEnd('\\')})";
                        StorageSizeInfo = $"{FormatSize(freeBytes)} disponibles de {FormatSize(totalBytes)}";
                        StorageValue = usedBytes / totalBytes * 100;
                    }

                    UpdateList(path);
                }
            }
            catch (Exception)
            {
                DiskLabel = "Estado del disco no disponible";
                StorageSizeInfo = "Error al leer la unidad de almacenamiento";
                StorageValue = 0;
            }
        }
    }

    [RelayCommand]
    private void RefreshFileList()
    {
        string? path = DataHelper.Settings?.Server?.Path;
        UpdateList(path);
    }

    [RelayCommand]
    private static void OpenFolder()
    {
        string? path = DataHelper.Settings?.Server?.Path;
        if (!string.IsNullOrEmpty(path))
        {
            _ = Launcher.LaunchFolderPathAsync(path);
        }
    }

    private static string FormatSize(double bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];

        double size = bytes;

        int unitIndex = 0;
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024.0;
            unitIndex++;
        }

        string format = unitIndex == 0 ? "N0" : "N2";
        return $"{size.ToString(format)} {units[unitIndex]}";
    }

    private void UpdateList(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        string directoryPath = path;
        var directoryInfo = new DirectoryInfo(directoryPath);

        ServerFiles.Clear();

        try
        {
            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                ServerFiles.Add(new ServerFile(
                    name: dir.Name,
                    size: "--",
                    modifiedDate: dir.LastWriteTime.ToString("dd/MM/yyyy HH:mm"),
                    absolutePath: dir.FullName,
                    isFile: false,
                    icon: new SymbolIcon(Symbol.Folder)
                ));
            }

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                ServerFiles.Add(new ServerFile(
                    name: file.Name,
                    size: FormatSize(file.Length),
                    modifiedDate: file.LastWriteTime.ToString("dd/MM/yyyy HH:mm"),
                    absolutePath: file.FullName,
                    isFile: true,
                    icon: new SymbolIcon(Symbol.Document)
                ));
            }
        }
        catch (UnauthorizedAccessException)
        {
        }

    }

    public void Receive(AppSettingsChangedMessage message)
    {
        string? path = DataHelper.Settings?.Server?.Path;
        OnPathChanged(path);
    }
}
