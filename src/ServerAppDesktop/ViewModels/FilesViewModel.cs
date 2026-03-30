namespace ServerAppDesktop.ViewModels;

public sealed partial class FilesViewModel : ObservableRecipient, IRecipient<AppSettingsChangedMessage>
{
    private readonly IFilesService _fileService;
    private readonly DispatcherTimer _refreshTimer;
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();
    private string _currentPath = DataHelper.Settings?.Server?.Path ?? "";

    public event EventHandler<ErrorOperationEventArgs>? ErrorOccurred;
    public event EventHandler<SuccessOperationEventArgs>? OperationSuccess;

    private void OnError(string m, string d = "") => ErrorOccurred?.Invoke(this, new ErrorOperationEventArgs(m, d));
    private void OnSuccess(string m) => OperationSuccess?.Invoke(this, new SuccessOperationEventArgs(m));

    [ObservableProperty] private bool _isConfigured = false;

    partial void OnIsConfiguredChanged(bool value)
    {
        CanRefresh = value;
        CanCreateFileOrFolder = value;
        CanMakeBackup = value;
        if (value && !string.IsNullOrEmpty(_currentPath))
        { OnPathChanged(_currentPath); _refreshTimer.Start(); }
        else if (!value)
        { _refreshTimer.Stop(); ServerFiles.Clear(); ResetDiskMetrics(); }
    }

    [ObservableProperty] private string _diskLabel = "";
    [ObservableProperty] private string _storageSizeInfo = "";
    [ObservableProperty] private double _storageValue = 0;
    [ObservableProperty] private bool _canGoBack = false;
    [ObservableProperty] private bool _canGoForward = false;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(MakeBackupCommand))] private bool _canMakeBackup = true;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(OpenFolderCommand))] private bool _canOpenFolder = true;
    [ObservableProperty] private ServerFile? _selectedServerFile;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CopyCommand))] private bool _canCopy = false;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(PasteCommand))] private bool _canPaste = false;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CutCommand))] private bool _canCut = false;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RenameCommand))][NotifyCanExecuteChangedFor(nameof(DeleteCommand))][NotifyCanExecuteChangedFor(nameof(PropertiesCommand))] private bool _canModifySelected = false;
    [ObservableProperty] private bool _canRefresh = false;
    [ObservableProperty] private bool _canViewPropertiesOfFileOrFolder = false;
    [ObservableProperty] private bool _canCreateFileOrFolder = false;

    public ObservableCollection<ServerFile> ServerFiles { get; } = [];

    public FilesViewModel(IFilesService fileService)
    {
        _fileService = fileService;
        IsActive = true;
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _refreshTimer.Tick += (s, e) => { if (IsConfigured) { CanPaste = ClipboardHelper.HasFileContent(); RefreshFileList(); } };
        IsConfigured = !string.IsNullOrEmpty(_currentPath) && Directory.Exists(_currentPath);
    }

    private void NavigateTo(string newPath, bool isHistory = false)
    {
        if (!IsConfigured || string.IsNullOrEmpty(newPath) || !Directory.Exists(newPath))
            return;
        if (!isHistory && !string.IsNullOrEmpty(_currentPath))
        { _backStack.Push(_currentPath); _forwardStack.Clear(); }
        _currentPath = newPath;
        OnPathChanged(_currentPath);
        CanGoBack = _backStack.Count > 0;
        CanGoForward = _forwardStack.Count > 0;
        GoBackCommand.NotifyCanExecuteChanged();
        GoForwardCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() { if (_backStack.Count > 0) { _forwardStack.Push(_currentPath); NavigateTo(_backStack.Pop(), true); } }

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward() { if (_forwardStack.Count > 0) { _backStack.Push(_currentPath); NavigateTo(_forwardStack.Pop(), true); } }

    [RelayCommand]
    private async Task SetPath()
    {
        if (SelectedServerFile == null || !IsConfigured)
            return;
        if (!SelectedServerFile.IsFile)
            NavigateTo(SelectedServerFile.AbsolutePath);
        else
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(SelectedServerFile.AbsolutePath);
                await Launcher.LaunchFileAsync(file);
            }
            catch (Exception ex)
            {
                OnError(string.Format(ResourceHelper.GetString("Files_Err_Open"), SelectedServerFile.Name), ex.Message);
            }
        }
    }

    [RelayCommand]
    private void RefreshFileList() { if (IsConfigured) OnPathChanged(_currentPath); }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void Copy()
    {
        if (SelectedServerFile == null || !IsConfigured)
            return;
        try
        {
            if (SelectedServerFile.IsFile)
                ClipboardHelper.SetFile(SelectedServerFile.AbsolutePath);
            else
                ClipboardHelper.SetFolder(SelectedServerFile.AbsolutePath);
            CanPaste = true;
            PasteCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex) { OnError(ResourceHelper.GetString("Files_Err_Clipboard"), ex.Message); }
    }

    [RelayCommand(CanExecute = nameof(CanPaste))]
    private async Task Paste()
    {
        if (!IsConfigured || string.IsNullOrEmpty(_currentPath))
            return;
        string[] sources = ClipboardHelper.GetPaths();
        if (sources.Length == 0)
            return;
        try
        {
            bool isMove = ClipboardHelper.GetDropEffect() == 2;
            await _fileService.CopyAsync(sources, _currentPath, isMove);
            RefreshFileList();
            OnSuccess(ResourceHelper.GetString(isMove ? "Files_Success_Move" : "Files_Success_Copy"));
        }
        catch (Exception ex) { OnError(ResourceHelper.GetString("Files_Err_Paste"), ex.Message); }
    }

    [RelayCommand(CanExecute = nameof(CanCut))]
    private void Cut() { try { Copy(); ClipboardHelper.SetMoveEffect(); } catch (Exception ex) { OnError(ResourceHelper.GetString("Files_Err_MovePrepare"), ex.Message); } }

    partial void OnSelectedServerFileChanged(ServerFile? value) { bool state = value != null && IsConfigured; CanCopy = state; CanCut = state; CanModifySelected = state; CanViewPropertiesOfFileOrFolder = state; }

    private async void OnPathChanged(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        { ResetDiskMetrics(); ServerFiles.Clear(); return; }
        CanRefresh = CanCreateFileOrFolder = CanMakeBackup = IsConfigured;
        CanPaste = IsConfigured && ClipboardHelper.HasFileContent();
        UpdateDiskUI(path);
        await UpdateListAsync(path);
        PasteCommand.NotifyCanExecuteChanged();
    }

    private void UpdateDiskUI(string path)
    {
        DriveInfo? info = _fileService.GetDriveInfo(path);
        if (info != null)
        {
            double total = info.TotalSize;
            double free = info.AvailableFreeSpace;
            string label = string.IsNullOrEmpty(info.VolumeLabel) ? ResourceHelper.GetString("Files_Disk_Local") : info.VolumeLabel;
            DiskLabel = $"{label} ({info.Name.TrimEnd('\\')})";
            StorageSizeInfo = string.Format(ResourceHelper.GetString("Files_Disk_StorageInfo"), _fileService.FormatSize(free), _fileService.FormatSize(total));
            StorageValue = (total - free) / total * 100;
        }
        else
            ResetDiskMetrics();
    }

    private void ResetDiskMetrics() { CanPaste = CanRefresh = CanCreateFileOrFolder = CanMakeBackup = false; DiskLabel = ResourceHelper.GetString("Files_Disk_Inaccessible"); StorageSizeInfo = "--"; StorageValue = 0; }

    private async Task UpdateListAsync(string path)
    {
        try
        {
            List<ServerFile> current = await _fileService.GetFilesAsync(path);
            var toRemove = ServerFiles.Where(sf => current.All(c => c.AbsolutePath != sf.AbsolutePath)).ToList();
            foreach (ServerFile? r in toRemove)
                ServerFiles.Remove(r);
            for (int i = 0; i < current.Count; i++)
            {
                ServerFile c = current[i];
                ServerFile? exist = ServerFiles.FirstOrDefault(sf => sf.AbsolutePath == c.AbsolutePath);
                if (exist != null)
                {
                    int oldIdx = ServerFiles.IndexOf(exist);
                    if (oldIdx != i)
                        ServerFiles.Move(oldIdx, i);
                    if (exist.ModifiedDate != c.ModifiedDate || exist.Size != c.Size)
                    { exist.ModifiedDate = c.ModifiedDate; exist.Size = c.Size; }
                }
                else
                    ServerFiles.Insert(i, c);
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
    }

    [RelayCommand] private void OpenFolder() { string? p = DataHelper.Settings?.Server?.Path; if (!string.IsNullOrEmpty(p)) _ = Launcher.LaunchFolderPathAsync(p); }

    [RelayCommand(CanExecute = nameof(CanMakeBackup))]
    private async Task MakeBackupAsync()
    {
        CanMakeBackup = false;
        string? s = DataHelper.Settings?.Server?.Path;
        var picker = new FolderPicker(MainWindow.Instance.AppWindow.Id) { SuggestedStartLocation = PickerLocationId.ComputerFolder };
        PickFolderResult target = await picker.PickSingleFolderAsync();

        if (target != null && s != null)
        {
            try
            {
                string folderName = new DirectoryInfo(s).Name;
                string dest = Path.Combine(target.Path, folderName + "-backup-" + DateTime.Now.ToString("yyyyMMdd-HHmm"));
                await _fileService.CopyAsync([s], dest, false);
                OnSuccess(string.Format(ResourceHelper.GetString("Files_Success_Backup"), target.Path));
            }
            catch (Exception ex) { OnError(ResourceHelper.GetString("Files_Err_Backup"), ex.Message); }
        }
        CanMakeBackup = true;
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task Rename(string? name)
    {
        if (SelectedServerFile == null || string.IsNullOrWhiteSpace(name))
            return;

        string oldName = SelectedServerFile.Name;
        try
        {
            await _fileService.RenameAsync(SelectedServerFile.AbsolutePath, name, SelectedServerFile.IsFile);
            RefreshFileList();
            OnSuccess(string.Format(ResourceHelper.GetString("Files_Success_Rename"), oldName, name));
        }
        catch (Exception ex) { OnError(string.Format(ResourceHelper.GetString("Files_Err_Rename"), oldName), ex.Message); }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private void Properties() { if (SelectedServerFile != null) ServerUIHelper.ShowFileProperties(SelectedServerFile.AbsolutePath, MainWindow.Instance); }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteAsync()
    {
        if (SelectedServerFile == null)
            return;

        string itemName = SelectedServerFile.Name;
        try
        {
            await _fileService.DeleteAsync(SelectedServerFile.AbsolutePath, SelectedServerFile.IsFile);
            OnSuccess(string.Format(ResourceHelper.GetString("Files_Success_Delete"), itemName));
            SelectedServerFile = null;
            RefreshFileList();
        }
        catch (Exception ex) { OnError(string.Format(ResourceHelper.GetString("Files_Err_Delete"), itemName), ex.Message); }
    }

    [RelayCommand]
    private async Task CreateFile(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;
        try
        {
            await _fileService.CreateItemAsync(_currentPath, name, true);
            OnSuccess(string.Format(ResourceHelper.GetString("Files_Success_CreateFile"), name));
            RefreshFileList();
        }
        catch (Exception ex) { OnError(string.Format(ResourceHelper.GetString("Files_Err_CreateFile"), name), ex.Message); }
    }

    [RelayCommand]
    private async Task CreateFolder(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;
        try
        {
            await _fileService.CreateItemAsync(_currentPath, name, false);
            OnSuccess(string.Format(ResourceHelper.GetString("Files_Success_CreateFolder"), name));
            RefreshFileList();
        }
        catch (Exception ex) { OnError(string.Format(ResourceHelper.GetString("Files_Err_CreateFolder"), name), ex.Message); }
    }

    public void Receive(AppSettingsChangedMessage m) { _currentPath = DataHelper.Settings?.Server?.Path ?? ""; IsConfigured = !string.IsNullOrEmpty(_currentPath) && Directory.Exists(_currentPath); OnPathChanged(_currentPath); }
}
