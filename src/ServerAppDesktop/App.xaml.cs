namespace ServerAppDesktop;

public sealed partial class App : Application
{
    public static IHost? Host { get; private set; }
    private readonly bool _trayOnly;

    public App(bool trayOnly = false)
    {
        _trayOnly = trayOnly;
        InitializeComponent();

        if (SettingsHelper.ExistsConfigurationFile())
        {
            SettingsHelper.LoadAndSetBasicSettings();
        }
        else
        {
            DataHelper.Settings = null;
        }

        Host = AppHandler.ConfigureHost();
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        return Host!.Services.GetRequiredService<T>() ?? throw new InvalidOperationException("Host no inicializado");
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
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
            EfficiencyModeUtilities.SetEfficiencyMode(true);
            handler.WindowHidden = true;
        }

        if (MainWindow.Instance != null)
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
        }
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
