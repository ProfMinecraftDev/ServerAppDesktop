using System;
using System.Collections.Generic;

namespace ServerAppDesktop.Models
{
    public class ReleaseInfo
    {
        public string TagName { get; set; } = "";
        public string Name { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime PublishedAt { get; set; } = DateTime.MinValue;
        public bool Prerelease { get; set; } = false;
        public List<Asset> Assets { get; set; } = new();

        public Asset GetExeAsset()
        {
            return Assets.Find(a => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                   ?? new Asset();
        }
    }

    public class Asset
    {
        public string Name { get; set; } = "";
        public string BrowserDownloadUrl { get; set; } = "";
    }
}
