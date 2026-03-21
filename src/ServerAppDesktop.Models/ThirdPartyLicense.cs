namespace ServerAppDesktop.Models;

public sealed class ThirdPartyLicense(
    string nameProduct = "",
    string licenseProduct = "",
    Uri? licenseUrl = null,
    string officialWeb = "",
    Uri? url = null)
{
    public string NameProduct { get; set; } = nameProduct;
    public string LicenseProduct { get; set; } = licenseProduct;
    public Uri? LicenseUrl { get; set; } = licenseUrl;
    public string OfficialWeb { get; set; } = officialWeb;
    public Uri? Url { get; set; } = url;
}
