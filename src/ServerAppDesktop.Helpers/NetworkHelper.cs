
namespace ServerAppDesktop.Helpers
{
    public static class NetworkHelper
    {
        private static string _cachedInterfaceName = "";
        public static event Action<bool>? ConnectionChanged;

        static NetworkHelper()
        {
            NetworkInformation.NetworkStatusChanged += OnNewtworkStatusChanged;
        }

        private static async void OnNewtworkStatusChanged(object sender)
        {
            bool isConnected = await IsInternetAvailableAsync();
            ConnectionChanged?.Invoke(isConnected);
        }

        public static async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(5);
                HttpResponseMessage response = await client.GetAsync("http://www.microsoft.com");
                ConnectionChanged?.Invoke(response.IsSuccessStatusCode);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                ConnectionChanged?.Invoke(false);
                return false;
            }
        }

        public static string GetNetworkInterfaceName()
        {
            ConnectionProfile currentProfile = NetworkInformation.GetInternetConnectionProfile();

            if (currentProfile != null && currentProfile.NetworkAdapter != null)
            {
                Guid adapterId = currentProfile.NetworkAdapter.NetworkAdapterId;

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.Id.Equals(adapterId.ToString("B"), StringComparison.OrdinalIgnoreCase))
                    {
                        if (_cachedInterfaceName == ni.Description)
                            return ni.Description;

                        _cachedInterfaceName = ni.Description;
                        return ni.Description;
                    }
                }
            }
            _cachedInterfaceName = "null";
            return "null";
        }
    }
}
