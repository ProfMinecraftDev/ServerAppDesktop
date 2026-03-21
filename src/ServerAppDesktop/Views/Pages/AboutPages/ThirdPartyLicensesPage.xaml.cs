namespace ServerAppDesktop.Views.Pages.AboutPages;

public sealed partial class ThirdPartyLicensesPage : Page
{
    public ObservableCollection<ThirdPartyLicense> Licenses { get; } = [];

    public ThirdPartyLicensesPage()
    {
        InitializeComponent();
        Licenses.Add(new ThirdPartyLicense(
            "Server App Desktop",
            "MIT",
            new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop/blob/master/LICENSE"),
            "Repositorio Oficial",
            new Uri("https://github.com/ProfMinecraftDev/ServerAppDesktop")));

        Licenses.Add(new ThirdPartyLicense(
            "Microsoft .NET SDK (net10.0)",
            "MIT",
            new Uri("https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"),
            "Official .NET Website",
            new Uri("https://dotnet.microsoft.com/")));

        Licenses.Add(new ThirdPartyLicense(
            "Windows App SDK",
            "MIT",
            new Uri("https://github.com/microsoft/WindowsAppSDK/blob/main/LICENSE"),
            "GitHub Repository",
            new Uri("https://github.com/microsoft/WindowsAppSDK")));

        Licenses.Add(new ThirdPartyLicense(
            "CommunityToolkit.Mvvm",
            "MIT",
            new Uri("https://github.com/CommunityToolkit/dotnet/blob/main/License.md"),
            "Community Toolkit GitHub",
            new Uri("https://github.com/CommunityToolkit/dotnet")));

        Licenses.Add(new ThirdPartyLicense(
            "CommunityToolkit.WinUI.Controls",
            "MIT",
            new Uri("https://github.com/CommunityToolkit/Windows/blob/main/License.md"),
            "Settings & Extensions",
            new Uri("https://github.com/CommunityToolkit/Windows")));

        Licenses.Add(new ThirdPartyLicense(
            "CommunityToolkit.Labs.WinUI",
            "MIT",
            new Uri("https://github.com/CommunityToolkit/Labs-Windows/blob/main/License.md"),
            "DataTable & Markdown Controls",
            new Uri("https://github.com/CommunityToolkit/Labs-Windows")));

        Licenses.Add(new ThirdPartyLicense(
            "WinUIEx",
            "Apache-2.0",
            new Uri("https://github.com/dotMorten/WinUIEx/blob/main/LICENSE"),
            "WinUIEx GitHub",
            new Uri("https://github.com/dotMorten/WinUIEx")));

        Licenses.Add(new ThirdPartyLicense(
            "DevWinUI.Controls",
            "MIT",
            new Uri("https://github.com/ghost1372/DevWinUI/blob/main/LICENSE"),
            "DevWinUI GitHub",
            new Uri("https://github.com/ghost1372/DevWinUI")));

        Licenses.Add(new ThirdPartyLicense(
            "Microsoft.Extensions.Hosting",
            "MIT",
            new Uri("https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"),
            "Generic Host Services",
            new Uri("https://dotnet.microsoft.com/")));

        Licenses.Add(new ThirdPartyLicense(
            "Microsoft.Windows.CsWin32",
            "MIT",
            new Uri("https://github.com/microsoft/CsWin32/blob/main/LICENSE"),
            "P/Invoke Source Generator",
            new Uri("https://github.com/microsoft/CsWin32")));

        Licenses.Add(new ThirdPartyLicense(
            "System.Drawing.Common",
            "MIT",
            new Uri("https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"),
            "GDI+ Graphics System",
            new Uri("https://github.com/dotnet/runtime")));

        Licenses.Add(new ThirdPartyLicense(
            "ProfMinecraftDev.CSharpEx",
            "MIT",
            new Uri("https://github.com/ProfMinecraftDev/CSharpEx/blob/master/LICENSE.txt"),
            "NuGet Package",
            new Uri("https://www.nuget.org/packages/ProfMinecraftDev.CSharpEx/")));

        Licenses.Add(new ThirdPartyLicense(
            "Icons8",
            "Universal Multimedia License",
            new Uri("https://icons8.com/license"),
            "Icons8 Assets",
            new Uri("https://icons8.com/")));

        Licenses.Add(new ThirdPartyLicense(
            "GitHub",
            "N/A",
            null,
            "Project Hosting",
            new Uri("https://github.com/")));

        Licenses.Add(new ThirdPartyLicense(
            "Windows SDK Build Tools",
            "Microsoft Software License",
            null,
            "Windows Development Tools",
            new Uri("https://developer.microsoft.com/windows/")));
    }
}
