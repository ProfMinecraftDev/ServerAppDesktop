namespace ServerAppDesktop;

public sealed partial class App : Application
{
    public static IHost? Host { get; private set; }
    private readonly bool _trayOnly;

    public App(bool trayOnly = false)
    {
        _trayOnly = trayOnly;

        if (SettingsHelper.ExistsConfigurationFile())
        {
            SettingsHelper.LoadAndSetBasicSettings();
        }
        else
        {
            DataHelper.Settings = null;
        }

        DataHelper.Initialize();
        InitializeComponent();

        Host = AppHandler.ConfigureHost();
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        return Host!.Services.GetRequiredService<T>() ?? throw new InvalidOperationException("Host no inicializado");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        string windows10IconPath = "M0 36.357L104.62 22.11l.045 100.914-104.57.595L0 36.358zm104.57 98.293l.08 101.002L.081 221.275l-.006-87.302 104.494.677zm12.682-114.405L255.968 0v121.74l-138.716 1.1V20.246zM256 135.6l-.033 121.191-138.716-19.578-.194-101.84L256 135.6z";
        string windows11IconPath = "M0 5860 l0 -1820 1820 0 1820 0 0 1820 0 1820 -1820 0 -1820 0 0 -1820z M4040 5860 l0 -1820 1820 0 1820 0 0 1820 0 1820 -1820 0 -1820 0 0 -1820z M0 1820 l0 -1820 1820 0 1820 0 0 1820 0 1820 -1820 0 -1820 0 0 -1820z M4040 1820 l0 -1820 1820 0 1820 0 0 1820 0 1820 -1820 0 -1820 0 0 -1820z";

        bool isWindows11 = Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000;

        if (isWindows11)
            Resources["WindowsIconPath"] = windows11IconPath;
        else
            Resources["WindowsIconPath"] = windows10IconPath;
        WindowManager.PersistenceStorage = new FilePersistence(Path.Join(DataHelper.SettingsPath, DataHelper.WindowPersistenceFile));
        MainWindow.Initialize();

        _ = GetRequiredService<SettingsViewModel>();
        IWindowHandler handler = GetRequiredService<IWindowHandler>();

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
            handler.WindowHidden = false;
        }
        else
        {
            ProcessHelper.SetEfficiencyMode(true);
            handler.WindowHidden = true;
        }

        MainWindow.Instance.Content.To<FrameworkElement>().Loaded += async (_, _) =>
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
        };
    }
}

internal partial class FilePersistence : IDictionary<string, object>
{
    private readonly Dictionary<string, object> _data = [];
    private readonly string _file;

    public FilePersistence(string filename)
    {
        _file = filename;
        try
        {
            if (File.Exists(filename))
            {
                JsonObject jo = JsonObject.Parse(File.ReadAllText(filename)).To<JsonObject>();
                foreach (KeyValuePair<string, JsonNode?> node in jo)
                {
                    if (node.Value is JsonValue jvalue && jvalue.TryGetValue<string>(out string? value))
                    {
                        _data[node.Key] = value ?? "";
                    }
                }
            }
        }
        catch { }
    }
    private void Save()
    {
        JsonObject jo = [];
        foreach (KeyValuePair<string, object> item in _data)
        {
            if (item.Value is string s)
            {
                jo.Add(item.Key, s);
            }
        }
        File.WriteAllText(_file, jo.ToJsonString());
    }
    public object this[string key] { get => _data[key]; set { _data[key] = value; Save(); } }

    public ICollection<string> Keys => _data.Keys;

    public ICollection<object> Values => _data.Values;

    public int Count => _data.Count;

    public bool IsReadOnly => false;

    public void Add(string key, object value)
    {
        _data.Add(key, value);
        Save();
    }

    public void Add(KeyValuePair<string, object> item)
    {
        _data.Add(item.Key, item.Value);
        Save();
    }

    public void Clear()
    {
        _data.Clear();
        Save();
    }

    public bool Contains(KeyValuePair<string, object> item)
    {
        return _data.Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return _data.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        throw new NotImplementedException(); // TODO
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        throw new NotImplementedException(); // TODO
    }

    public bool Remove(string key)
    {
        throw new NotImplementedException(); // TODO
    }

    public bool Remove(KeyValuePair<string, object> item)
    {
        throw new NotImplementedException(); // TODO
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        throw new NotImplementedException(); // TODO
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException(); // TODO
    }
}
