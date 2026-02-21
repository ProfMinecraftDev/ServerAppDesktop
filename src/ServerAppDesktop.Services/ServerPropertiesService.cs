

namespace ServerAppDesktop.Services
{
    public class ServerPropertiesService : IServerPropertiesService
    {
        private const string SERVER_PROPERTIES_FILE = "server.properties";
        private Dictionary<string, object> _properties = [];
        private string _serverPath = "";

        public ServerPropertiesService()
        {
        }

        private void SaveFile()
        {
            string filePath = Path.Combine(_serverPath, SERVER_PROPERTIES_FILE);

            IEnumerable<string> lines = _properties.Select(kvp => $"{kvp.Key}={kvp.Value}");

            TimeZoneInfo localZone = TimeZoneInfo.Local;
            string tzAbbreviation = localZone.IsDaylightSavingTime(DateTime.Now)
                ? localZone.DaylightName
                : localZone.StandardName;
            string zone = tzAbbreviation.Length > 4 ? "UTC" : tzAbbreviation;

            string timestamp = DateTime.Now.ToString($"ddd MMM dd HH:mm:ss {zone} yyyy",
                                    CultureInfo.InvariantCulture);

            List<string> content =
            [
                "# Minecraft Server Properties",
                $"#{timestamp}",
                .. lines,
            ];

            try
            {
                File.WriteAllLines(filePath, content);
            }
            catch { return; }
        }

        public void SetPath(string serverPath)
        {
            _serverPath = serverPath;
            LoadFile();
        }

        public void LoadFile()
        {
            string filePath = Path.Combine(_serverPath, SERVER_PROPERTIES_FILE);
            if (!File.Exists(filePath))
            {
                return;
            }

            _properties = File.ReadAllLines(filePath)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#') && !l.StartsWith('!'))
                .Select(l => l.Split(['=', ':'], 2))
                .Where(p => p.Length == 2)
                .ToDictionary(
                    p => p[0].Trim(),
                    p => (object)p[1].Trim()
                );
        }

        public T? GetValue<T>(string key)
        {
            if (_properties.TryGetValue(key, out object? value))
            {
                try
                {
                    return Convert.ChangeType(value, typeof(T)).To<T>();
                }
                catch
                {
                    return default;
                }
            }

            return default;
        }

        public object? GetValue(string key) => GetValue<object>(key);

        public void SetValue(string key, object value)
        {
            _properties[key] = value;
            SaveFile();
        }
    }
}
