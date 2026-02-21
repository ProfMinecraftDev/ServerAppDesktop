
namespace ServerAppDesktop.Helpers
{
    public sealed partial class ResourceHelper
    {
        private static readonly ResourceLoader _resourceLoader = new();

        public static string GetString(string resourceKey)
        {
            return _resourceLoader.GetString(resourceKey);
        }
    }
}
