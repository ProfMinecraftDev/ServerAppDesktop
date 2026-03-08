

namespace ServerAppDesktop.Helpers;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(List<ReleaseInfo>))]
[JsonSerializable(typeof(ReleaseInfo))]
internal partial class UpdateJsonContext : JsonSerializerContext { }

public static class UpdateHelper
{
    public static event EventHandler<DownloadProgressChangedEventArgs>? DownloadProgressChanged;
    private const string GITHUB_API_RELEASES = "https://api.github.com/repos/{0}/{1}/releases";

    public static async Task<ReleaseInfo?> GetUpdateAsync(string username, string repository, string currentVersion, bool isPreRelease)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (string.IsNullOrEmpty(repository))
        {
            throw new ArgumentNullException(nameof(repository));
        }

        if (string.IsNullOrEmpty(currentVersion))
        {
            throw new ArgumentNullException(nameof(currentVersion));
        }

        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", username);

        string url = string.Format(GITHUB_API_RELEASES, username, repository);
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        _ = response.EnsureSuccessStatusCode();

        string responseJson = await response.Content.ReadAsStringAsync();
        List<ReleaseInfo>? releases = JsonSerializer.Deserialize(responseJson, UpdateJsonContext.Default.ListReleaseInfo);

        if (releases == null || releases.Count == 0)
        {
            return null;
        }

        ReleaseInfo? release = releases
            .Where(r => isPreRelease ? r.IsPreRelease : !r.IsPreRelease)
            .OrderByDescending(r => r.PublishedAt)
            .FirstOrDefault();

        return release != null && release.VersionTag == currentVersion ? null : release;
    }

    public static async Task<bool> DownloadUpdateAsync(Asset updateFile)
    {
        string tempFolder = Path.Combine(Path.GetTempPath(), "ServerAppDesktop_Updates");
        if (!Directory.Exists(tempFolder))
        {
            _ = Directory.CreateDirectory(tempFolder);
        }

        string tempPath = Path.Combine(tempFolder, updateFile.Name);

        if (File.Exists(tempPath) && await CompareHash(tempPath, updateFile.SHA256))
        {
            RegisterInstallation(tempPath);
            return true;
        }

        using HttpClient httpClient = new();
        using HttpResponseMessage response = await httpClient.GetAsync(updateFile.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
        _ = response.EnsureSuccessStatusCode();

        long totalBytes = response.Content.Headers.ContentLength ?? -1L;
        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        using FileStream fileStream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        byte[] buffer = new byte[8192];
        long totalRead = 0;
        int read;

        while ((read = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read));
            totalRead += read;

            if (totalBytes != -1)
            {
                double percentage = (double)totalRead / totalBytes * 100;
                string progressText = $"{percentage:F0}% ({totalRead / 1048576.0:0.##} MB / {totalBytes / 1048576.0:0.##} MB)";
                DownloadProgressChanged?.Invoke(null, new DownloadProgressChangedEventArgs(progressText, percentage));
            }
        }

        await fileStream.FlushAsync();
        fileStream.Close();

        if (await CompareHash(tempPath, updateFile.SHA256))
        {
            RegisterInstallation(tempPath);
            return true;
        }

        return false;
    }

    private static void RegisterInstallation(string tempPath)
    {


        ProcessStartInfo startInfo = new()
        {
            FileName = tempPath,
            Arguments = "/SILENT /SUPPRESSMSGBOXES /NORESTART /SP- /NORESTARTAPPLICATIONS /FORCECLOSEAPPLICATIONS /RUN",
            UseShellExecute = true,
            Verb = "runas",
        };

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            try
            {
                _ = Process.Start(startInfo);
            }
            catch { }
        };
    }

    private static async Task<bool> CompareHash(string filePath, string hashToCompare)
    {
        if (string.IsNullOrEmpty(hashToCompare))
        {
            return false;
        }

        using var sha256 = SHA256.Create();
        using FileStream stream = File.OpenRead(filePath);
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        string sha256LocalFile = Convert.ToHexString(hashBytes);

        return string.Equals(sha256LocalFile, hashToCompare, StringComparison.OrdinalIgnoreCase);
    }

    public static void CleanOldUpdates()
    {
        try
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), "ServerAppDesktop_Updates");
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }
        catch { }
    }

    public static async Task<string> GetNewsOfLatestRelease(string username, string repository, bool isPreRelease)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (string.IsNullOrEmpty(repository))
        {
            throw new ArgumentNullException(nameof(repository));
        }

        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", username);

        string url = string.Format(GITHUB_API_RELEASES, username, repository);
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return "";
        }

        _ = response.EnsureSuccessStatusCode();

        string responseJson = await response.Content.ReadAsStringAsync();
        List<ReleaseInfo>? releases = JsonSerializer.Deserialize(responseJson, UpdateJsonContext.Default.ListReleaseInfo);

        if (releases == null || releases.Count == 0)
        {
            return "";
        }

        ReleaseInfo? release = releases
            .Where(r => isPreRelease ? r.IsPreRelease : !r.IsPreRelease)
            .OrderByDescending(r => r.PublishedAt)
            .FirstOrDefault();

        return release?.Notes ?? "";
    }
}
