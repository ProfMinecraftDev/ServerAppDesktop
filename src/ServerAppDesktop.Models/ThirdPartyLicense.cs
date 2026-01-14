using System;

namespace ServerAppDesktop.Models
{
    public sealed class ThirdPartyLicense
    {
        public string NameProduct { get; set; } = "";
        public string LicenseProduct { get; set; } = "";
        public Uri? LicenseUrl { get; set; }
        public string OfficialWeb { get; set; } = "";
        public Uri? Url { get; set; }
    }
}
