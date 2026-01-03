using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServerAppDesktop.Models
{
    public class ReleaseInfo
    {
        [JsonPropertyName("tag_name")]
        public string VersionTag { get; set; } = "";

        [JsonPropertyName("name")]
        public string Version { get; set; } = "";

        [JsonPropertyName("prerelease")]
        public bool IsPreRelease { get; set; } = false;

        [JsonPropertyName("body")]
        public string Notes { get; set; } = "";

        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonPropertyName("assets")]
        public List<Asset> Assets { get; set; } = [];
    }
}
