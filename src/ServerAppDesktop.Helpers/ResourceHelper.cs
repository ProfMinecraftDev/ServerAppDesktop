using Microsoft.Windows.ApplicationModel.Resources;

namespace ServerAppDesktop.Helpers
{
    public sealed partial class ResourceHelper
    {
        private readonly static ResourceLoader _resourceLoader = new();

        public static string GetString(string resourceKey)
        {
            return _resourceLoader.GetString(resourceKey);
        }
    }
}
