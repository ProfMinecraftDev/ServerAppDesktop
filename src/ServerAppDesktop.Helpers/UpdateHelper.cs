using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Helpers
{
    public class UpdateHelper
    {
        private readonly string owner;
        private readonly string repo;
        private readonly HttpClient client;

        public UpdateHelper(string owner, string repo)
        {
            this.owner = owner ?? "";
            this.repo = repo ?? "";
            client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ServerAppDesktop-Updater");
        }

        public async Task<UpdateResult> GetLatestExeAsync(string currentVersion, bool includePreRelease)
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases";
            var response = await client.GetStringAsync(url);

            var releases = JsonSerializer.Deserialize<List<ReleaseInfo>>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ReleaseInfo>();

            if (releases.Count == 0)
                return new UpdateResult(false, currentVersion, "", "", "", false);

            foreach (var rel in releases)
            {
                if (rel.Prerelease == includePreRelease)
                {
                    var exeAsset = rel.GetExeAsset();

                    bool isNewVersion = !string.Equals(currentVersion, rel.TagName, StringComparison.OrdinalIgnoreCase);

                    return new UpdateResult(
                        isNewVersion,
                        currentVersion,
                        rel.TagName ?? "",
                        rel.Body ?? "",
                        exeAsset.BrowserDownloadUrl ?? "",
                        rel.Prerelease
                    );
                }
            }

            return new UpdateResult(false, currentVersion, "", "", "", false);
        }
    }
}