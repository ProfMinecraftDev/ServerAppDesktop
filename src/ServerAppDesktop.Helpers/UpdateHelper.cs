using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Helpers
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(List<ReleaseInfo>))]
    [JsonSerializable(typeof(ReleaseInfo))]
    internal partial class UpdateJsonContext : JsonSerializerContext { }

    public static class UpdateHelper
    {
        public static event Action<string, double>? DownloadProgress;
        private const string GITHUB_API_RELEASES = "https://api.github.com/repos/{0}/{1}/releases";

        private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
        {
            TypeInfoResolver = UpdateJsonContext.Default
        };

        public static async Task<ReleaseInfo?> GetUpdateAsync(string username, string repository, string currentVersion, bool isPreRelease)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(repository))
                throw new ArgumentNullException(nameof(repository));
            if (string.IsNullOrEmpty(currentVersion))
                throw new ArgumentNullException(nameof(currentVersion));

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", username);

            string url = string.Format(GITHUB_API_RELEASES, username, repository);
            var response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            var releases = JsonSerializer.Deserialize(responseJson, UpdateJsonContext.Default.ListReleaseInfo);

            if (releases == null || releases.Count == 0)
                return null;

            var release = releases
                .Where(r => isPreRelease ? r.IsPreRelease : !r.IsPreRelease)
                .OrderByDescending(r => r.PublishedAt)
                .FirstOrDefault();

            if (release != null && release.VersionTag == currentVersion)
                return null;

            return release;
        }

        public static async Task<bool> DownloadUpdateAsync(Asset updateFile)
        {
            CleanOldUpdates();
            string tempFolder = Path.Combine(Path.GetTempPath(), "ServerAppDesktop_Updates");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string tempPath = Path.Combine(tempFolder, updateFile.Name);

            if (File.Exists(tempPath) && await CompareHash(tempPath, updateFile.SHA256))
            {
                RegisterInstallation(tempPath, tempFolder);
                return true;
            }

            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(updateFile.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalRead = 0;
            int read;

            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read);
                totalRead += read;

                if (totalBytes != -1)
                {
                    double percentage = (double)totalRead / totalBytes * 100;
                    string progressText = $"{percentage:F0}% ({totalRead / 1048576.0:0.##} MB / {totalBytes / 1048576.0:0.##} MB)";
                    DownloadProgress?.Invoke(progressText, percentage);
                }
            }

            await fileStream.FlushAsync();
            fileStream.Close();

            if (await CompareHash(tempPath, updateFile.SHA256))
            {
                RegisterInstallation(tempPath, tempFolder);
                return true;
            }

            return false;
        }

        private static void RegisterInstallation(string tempPath, string tempFolder)
        {
            // Lanzamos el proceso directamente. 
            // En WinUI 3 es más fiable que esperar al ProcessExit.
            var startInfo = new ProcessStartInfo
            {
                FileName = tempPath,
                Arguments = "/SILENT /SUPPRESSMSGBOXES /NORESTART /SP-",
                UseShellExecute = true,
                Verb = "runas",
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(tempFolder, "error.txt"), ex.Message);
            }
        }

        private static async Task<bool> CompareHash(string filePath, string hashToCompare)
        {
            if (string.IsNullOrEmpty(hashToCompare))
                return false;

            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
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
    }
}