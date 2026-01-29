using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace ServerAppDesktop.Helpers
{
    public static class NetworkHelper
    {
        private static string _cachedInterfaceName;
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
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync("http://www.google.com");
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
                var adapterId = currentProfile.NetworkAdapter.NetworkAdapterId;

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.Id.Equals(adapterId.ToString("B"), StringComparison.OrdinalIgnoreCase))
                    {
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
